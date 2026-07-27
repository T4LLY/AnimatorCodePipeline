# AI / UnityMCP ワークフロー

このガイドでは、AIとUnityMCPを使ってAnimator Code PipelineのModuleを作る基本的な流れを説明します。

AI環境、UnityMCP、Skillなどの導入は別のガイドで扱います。

ここでは、次の準備ができていることを前提にします。

- UnityプロジェクトをAIから編集できる
- UnityMCPが接続されている
- Animator Code Pipelineが導入されている
- Animator Code Pipeline用のSkillが利用できる

できていない人はこちら[AI環境セットアップ](ai-environment-setup.md)へ

最初は、実際にひとつ機能を作ってみましょう。

---

## 1. 今回作るもの

Unityで選択したGameObjectを、Expression MenuのRadial Puppetから縮小拡大できるようにします。

Radial Puppetの位置に応じて、

```text
最小    → 0.5倍
中間    → 1倍
最大    → 2倍
```

となるようにします。

Animation ClipやBlend Tree、Animator Parameterなどは、Animator Code Pipelineを使ってAIに作ってもらいます。

---

## 2. 作業の流れ

- UnityのHierarchyから、操作したいGameObjectを選択します。
- AIにしてもらいたい操作をプロンプトで伝えます。
- 選択したまま、AIへお願いしてみましょう。

---

## 3. AIへのお願い

AIのチャットに、次のように入力します。

>Unityで現在選択しているGameObjectを、Animator Code Pipelineを使って拡大・縮小できるようにしてください。
>Expression MenuのRadial Puppetで、最小を0.5倍、中間を1倍、最大を2倍にしてください。
>UnityMCPで現在の構成を確認し、必要なAnimator Code Pipelineの設定と、Modular AvatarのMenu・Parameter設定も行ってください。

これだけで始められます。

<details>
<summary>AIが実際にやっていること</summary>

AIはまずUnityMCPを使って現在のUnity Editorの状態を確認し、選択されているGameObjectが何なのか、そのGameObjectがどのAvatarに所属しているのか、Avatar Rootから見たHierarchy上の位置はどこなのか、対象にどのようなComponentが付いているのかなど、実装に必要な情報を実際のUnityプロジェクトから取得します。

さらに、Avatar内に既存の`AnimatorCodePipelineSettings`が存在するか、どの`AnimatorCodeModuleSet`が使用されているか、`Source Controller`として通常のAnimator Controllerが設定されているか、同じGameObjectに同じControllerを参照する`ModularAvatarMergeAnimator`がちょうど1つ存在するか、どのPlayable Layerへ統合する構成になっているか、既存のModuleに同じ機能や関連するParameterが存在しないかなどを確認し、既存の構成を再利用できる場合はそれを利用します。ローカルAnimatorは必須ではなく、アバター本体のAnimatorをSource Controllerの代わりに使用しません。

必要なAnimator Code Pipelineの構成がまだ存在しない場合は、現在のAvatar構成に合わせて必要なSettings、ModuleSet、プロジェクト所有の通常のSource Controller、同じホスト上のModular Avatar Merge Animator、必要に応じたModular Avatar Parameters/Menu関連コンポーネントを用意し、作成したModuleがNDMFビルド時に実行される状態まで設定します。Source ControllerとMerge AnimatorのController参照は同一にし、元のControllerやアバター本体のFX Controllerは直接変更しません。

そのうえで、選択したGameObjectを操作するための`AnimatorCodeModule`をC#で作成し、対象GameObjectのHierarchyパスを推測で書くのではなく、UnityMCPで確認した実際のAvatar相対パスを使用して対象を取得します。

今回のRadial Puppetによる拡大・縮小であれば、Radial Puppetから受け取る値を扱うためのFloat Parameterを定義し、そのParameterをAnimator側で利用できるようにし、選択したGameObjectを0.5倍、1倍、2倍へ変化させるために必要なAnimation ClipをAnimator As Codeから生成します。

さらに、それらのAnimation ClipをRadial Puppetの値に応じて連続的に補間できるようBlend Treeを構築し、入力値の最小、中間、最大に対応するMotionやThresholdを設定し、そのBlend Treeを再生するために必要なAnimator LayerやStateを作成します。

機能の構成によって複数のStateが必要な場合はStateを追加し、それらを接続するTransitionやTransition Condition、Parameterによる遷移条件、Exit Timeなども必要に応じて設定します。逆に、今回のようにひとつのBlend Tree Stateだけで実現できる構成であれば、不必要なStateやTransitionを増やさず、その機能に必要な構成だけを生成します。

Animator Layerについても、既存Moduleと共有すべきLayerがあるか、新しいLayerとして生成すべきかをAnimator Code Pipelineの構成に従って判断し、Layer名、State、Blend Tree、Parameter、Animation ClipなどがNDMFビルド時に一貫した形で生成されるようにします。

Expression側では、Animatorで使用するFloat Parameterを`ModularAvatarParameters`へ登録し、そのParameterを操作するRadial PuppetをExpression Menuへ追加します。Radial Puppetでは、ラジアル操作で入力したいParameterをRadial用パラメーター欄へ設定します。Sub Menuや他のPuppetでは、メニューを開いている間だけ有効になるParameterと、操作入力用のParameterが別になる場合があります。既存のMenuやParameter構成が存在する場合はそれを確認したうえで利用し、同じ用途のParameterやMenu項目を不必要に重複して作らないようにします。

Modular Avatarを利用できる部分についてはModular Avatarの非破壊な構成を利用し、元のVRChat Expression MenuやAnimator Controllerへ生成結果を直接書き込んで管理するのではなく、NDMFビルド時にAnimator Code Pipelineが生成したAnimatorとModular Avatar側のMenu・Parameter設定が最終的なAvatarへ統合される構成にします。

作成したC# Moduleは使用中の`AnimatorCodeModuleSet`へ登録し、そのModuleが対象の`AnimatorCodePipelineSettings`から実際に呼び出されることを確認します。複数のSettingsやModuleSetが存在する場合も、単に新しいものを追加するのではなく、現在のAvatar構成を確認して適切な場所へ追加します。

コードを作成した後はUnityのスクリプトコンパイル結果を確認し、C#のコンパイルエラー、参照しているAPIの間違い、存在しないHierarchyパス、必要なComponentの不足、ParameterやModuleの設定ミスなどがないかを確認します。エラーが発生した場合はUnityのConsoleや現在のプロジェクト状態を確認し、その原因に応じてModuleや設定を修正します。

最終的にはNDMFビルドを通して、Animator Code PipelineがModuleを実行し、Animator As Codeが必要なAnimation Clip、Parameter、Blend Tree、Layer、Stateなどを生成し、それらがModular Avatarを通してAvatarへ非破壊に統合されるところまで確認します。

つまり、人がUnity上で対象を選択して「Radial Puppetで0.5倍から2倍まで拡大・縮小したい」と伝えるだけで、対象GameObjectの特定、Hierarchyパスの取得、既存構成の調査、C# Moduleの作成、ModuleSetへの登録、Animator Parameterの定義、Animation Clipの生成、Blend Treeの構築、LayerやStateの生成、必要に応じたTransitionと遷移条件の設定、Expression Parameterの登録、Expression MenuへのRadial Puppetの追加、Modular Avatarとの連携、Unityでのコンパイル確認、NDMFビルドによる最終統合まで、一連の作業をAIに任せることができます。

そのため、Hierarchyパスを調べたり、Animator Controllerを開いてLayerを追加したり、Stateを配置したり、Animation Clipをひとつずつ作成したり、Blend TreeのThresholdを設定したり、Parameter名を各所で揃えたり、TransitionやConditionを設定したり、Expression Parametersへ同じParameterを登録したり、Expression MenuへRadial Puppetを作ったり、それらが正しく接続されているかをひとつずつ確認したりする必要はありません。

</details>

---

## 4. Gesture Managerで動かして確認しよう！

- Gesture Managerでアバターを再生し、Expression Menuに追加されたRadial Puppetを動かしてみます。
- 選択したオブジェクトがRadial Puppetに合わせて拡大縮小されれば完成です！


![AI開始時のログ](image\milfy_hair_scale.gif "サンプル")

[AIの実装レポート](ai-result.md)

---

## 5. 基本は「選んで、頼む」

このチュートリアルで人が行った操作は、ほぼ次の2つだけです。

```text
Unityで対象を選択
        ↓
AIにやりたいことを伝える
```

Animator Code PipelineのSkillとUnityMCPが利用できる状態なら、使い方に慣れてきたら次のような短い依頼でも構いません。

> 今選択しているオブジェクトを、Animator Code PipelineでRadial操作できるようにして。最小0.5倍、中間1倍、最大2倍にして。
