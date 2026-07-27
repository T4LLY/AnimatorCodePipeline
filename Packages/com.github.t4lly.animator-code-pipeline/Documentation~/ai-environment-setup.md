# AI環境セットアップ

このガイドでは、AIとUnity向けMCPを使ってAnimator Code Pipelineを利用するための環境を用意します。

ここでは、**ChatGPTの有料プランを利用していること**を前提にします。

以下のプロンプトをChatGPTに送ると、このガイドに沿って導入をサポートしてもらえます。

> ここのURL
>
> このガイドに沿って、Animator Code PipelineのAI環境構築を一つずつサポートしてください。  
> 私が行う必要のある操作だけ、その都度わかりやすく案内してください。


## 1. Animator Code PipelineをUnityへ入れる

まず、Animator Code Pipelineを使用するUnityプロジェクトへパッケージを追加します。

Animator Code PipelineのVPM Repositoryは次です。

```text
https://t4lly.github.io/vpm-repos/index.json
```

<details>
<summary>AI用指示</summary>

**これはAI用の指示です。ChatGPTへ送ってください。**

> VRChatのUnityプロジェクトにAnimator Code Pipelineを導入したいです。
>
> Animator Code PipelineのVPM Repositoryは次です。
>
> `https://t4lly.github.io/vpm-repos/index.json`
>
> まず、私がVRChatプロジェクトの管理にVCC（VRChat Creator Companion）とALCOMのどちらを使っているか確認してください。
>
> 私が分からない場合は、普段どのアプリからVRChatのUnityプロジェクトを開いているかなど、初心者でも答えられる質問を一つずつしてください。
>
> 使用しているツールが分かったら、そのツールにVPM Repositoryを追加し、Animator Code Pipelineを対象プロジェクトへ導入するところまで案内してください。
>
> 既にRepositoryやパッケージが登録されている可能性もあるため、分からない状態を推測せず、画面や現在の状態を確認しながら進めてください。
>
> 一度に大量の操作手順を出さず、必要な操作を一つずつ案内してください。
>
> Animator Code Pipelineの導入を案内する途中で、ModuleはC#コードとして実行されるため、他の人から受け取ったAnimator Code Pipeline用のModuleやスクリプトを、内容を確認せず安易にプロジェクトへ組み込まないようユーザーに説明してください。
>
> 「Animator Code Pipeline用だから安全」「知人からもらったから安全」とは判断せず、第三者が作成したC#コードは内容と出所を確認してから使用するよう案内してください。
>
> 特に、配布されたModuleやAIが別の環境向けに生成したスクリプトを、そのままコピーして使用することを推奨しないでください。
>
> 必要な機能は、可能であれば現在のUnityプロジェクトとAnimator Code PipelineのSkill・ドキュメントを確認したうえで、そのプロジェクト向けにAIへ作成させる方法を案内してください。
>
> 第三者のModuleを利用したい場合は、組み込む前にコードの内容をAIと一緒に確認するようユーザーへ案内してください。

</details>

## 2. Visual Studio Codeを用意する

次に、AIがプロジェクトのコードを確認・編集できるようにVisual Studio Code（VS Code）を用意します。

まだインストールされていない場合も、ChatGPTに案内してもらいます。

[Visual Studio Code 公式サイト](https://code.visualstudio.com/)

<details>
<summary>AI用指示</summary>

**これはAI用の指示です。ChatGPTへ送ってください。**

> このWindows PCでVisual Studio Codeを使えるようにしたいです。
>
> まずVS Codeが既にインストールされているか、初心者でも確認できる方法で案内してください。
>
> 既に使える場合は再インストールせず、そのまま次へ進めてください。
>
> インストールされていない場合は、Microsoft公式のVisual Studio Codeをダウンロードしてインストールする手順を一つずつ案内してください。
>
> インストーラーに選択項目が出た場合は、必要なものだけ簡単に説明してください。
>
> 最後にVS Codeを起動できるところまで案内してください。

</details>

## 3. Visual Studio CodeでCodexを使えるようにする

Visual Studio Codeが使えるようになったら、Codexを追加します。

Codexはコードの作成・編集や、ローカルのプロジェクトを調べながら作業するためのAIエージェントです。

ChatGPTアカウントでCodexへサインインできます。

- [ChatGPTプランでCodexを使う](https://help.openai.com/ja-jp/articles/11369540-chatgpt-%E3%83%97%E3%83%A9%E3%83%B3%E3%81%A7-codex-%E3%82%92%E4%BD%BF%E3%81%86)

<details>
<summary>AI用指示</summary>

**これはAI用の指示です。まずChatGPTへ送ってください。**

> Visual Studio CodeでOpenAI Codexを使えるようにしたいです。
>
> 現在の公式手順に従って、VS Code用のCodex IDE拡張機能をインストールし、私のChatGPTアカウントでサインインするところまで初心者向けに一つずつ案内してください。
>
> 既にCodexが使える状態なら、再インストールせず、正常に利用できることだけ確認してください。
>
> APIキーを新しく用意することを前提にせず、ChatGPTアカウントで利用できる方法を優先してください。

</details>

Codexでチャットできるようになったら、ここから先はCodexへ作業を引き継ぎます。

<details>
<summary>ここからCodexへ</summary>

ここまでChatGPTが人を案内

---

ここからCodexがPCとUnityプロジェクトを確認して作業

</details>

## 4. Codexに現在の環境を確認してもらう

ここからは、ソフトウェア、パッケージ、設定ファイルなどが存在するかを人が調べるのではなく、Codexに確認してもらいます。

まず、Animator Code Pipelineを使うUnityプロジェクトをVS Codeで開きます。

その状態で、次の指示をCodexへ送ります。

<details>
<summary>AI用指示</summary>

**これはAI用の指示です。VS CodeのCodexへ送ってください。**

> このUnityプロジェクトでAnimator Code PipelineをAIから扱える環境を整えます。
>
> まず変更を加えず、現在のPCとプロジェクトの状態を調査してください。
>
> 必要なソフトウェア、CLI、Unity Package、MCP設定、Skill、Gitなどについて、インストール済みであることを前提にしないでください。
>
> それぞれ実際に存在するか、現在どの状態なのかを確認してください。
>
> 既に存在するものについては、現在の構成をできるだけ利用できるように確認してください。
>
> ファイル名、インストール先、バージョン、設定場所などを推測せず、実際の環境から確認してください。
>
> 調査結果と、不足しているものを簡単にまとめてください。
>
> この段階では、まだ不足しているものをインストールしたり、既存の設定を変更したりしないでください。以降の手順に沿って一つずつ導入します。

</details>

## 5. Unity向けMCPを導入する

次に、AIからUnity Editorの状態を確認・操作できるようにMCPを導入します。

Unity向けMCPには、Unity公式のものや、CoplayDevの **MCP for Unity** などがあります。

参考:

- [Unity AI](https://docs.unity.com/en-us/ai)
- [MCP for Unity](https://github.com/CoplayDev/unity-mcp)

<details>
<summary>AI用指示</summary>

**これはAI用の指示です。VS CodeのCodexへ送ってください。**

> このVRChat Unityプロジェクトから、あなた自身がUnity Editorの状態を確認・操作できるMCP環境を用意してください。
>
> まず、現在開いているプロジェクトのUnityバージョンと、VRChatが現在対応しているUnityバージョンを確認してください。
>
> 次に、Unity公式のMCPが現在提供されているか、そのMCPがこのプロジェクトのUnityバージョンで利用できるか、現在の公式情報を確認してください。
>
> Unity公式MCPがこのVRChatプロジェクトで利用可能で、互換性にも問題がない場合は、原則として公式MCPを優先してください。
>
> Unity公式MCPが現在のUnityバージョンに対応していない、利用条件を満たさない、またはVRChatプロジェクトとの互換性に問題がある場合は、CoplayDevのMCP for Unityなど、この環境で利用できる適切な実装を選んでください。
>
> CoplayDevのMCP for Unityを使用する場合の公式リポジトリは次です。
>
> `https://github.com/CoplayDev/unity-mcp`
>
> MCPを利用するためだけにUnityをアップグレードしないでください。VRChatが対応しているUnityバージョンを優先してください。
>
> 既にMCPが導入され、正常に接続できている場合は、不必要に別のMCPへ入れ替えないでください。
>
> 使用するMCPを決めたら、必要なUnity Package、サーバー、ランタイム、CLI、設定ファイルなどが既に存在するか実際に確認してください。存在を推測しないでください。
>
> 既に利用できるものは再インストールせず、不足しているものだけを現在の公式手順に従って導入してください。
>
> Unity側の設定とCodex側のMCPクライアント設定の両方を確認し、接続できる状態にしてください。
>
> Unity Editor上でメニュー操作、ログイン、権限許可などユーザーによる操作が必要な場合だけ、その時点で私に分かりやすく案内してください。
>
> 接続できたら、MCPを使って現在Unityで選択されているGameObjectを取得してください。
>
> 実際に選択中のGameObjectを取得できれば、接続確認は完了です。

</details>

## 6. Animator Code Pipeline Skillを入れる

Unity向けMCPでUnityを確認できるようになったら、Animator Code Pipelineの作業方法をAIへ教えるSkillを追加します。

```text
Codex
   ├── Unity向けMCP
   │     └── Unityを見る・操作する
   │
   └── Animator Code Pipeline Skill
         └── ACPの作業方法を知る
```

<details>
<summary>AI用指示</summary>

**これはAI用の指示です。VS CodeのCodexへ送ってください。**

> Animator Code Pipeline用のSkillを、この環境のCodexから利用できるようにしてください。
>
> まず、現在のCodexで利用できるSkillの仕組みと、既存のSkill配置場所・設定を確認してください。
>
> Animator Code Pipeline用のSkillが既に利用可能なら、再配置せず内容と利用状態を確認してください。
>
> まだ利用できない場合は、Animator Code Pipelineに付属しているSkillを探し、この環境でCodexが利用できる適切な場所へ設定してください。
>
> Skillの場所やファイル名を推測せず、プロジェクト内の実際のファイルと現在のCodexの仕様を確認してください。
>
> 設定後、Animator Code PipelineのSkillを読み込み、このパッケージでModuleを作成するときの基本ルールを簡単に説明してください。

</details>

## 7. Gitも入れておきましょう

AIがファイルを変更するようになりました。

何を変更したのか確認できて、失敗しても元に戻せると安心です。

Gitを入れておきましょう。

<details>
<summary>AI用指示</summary>

**これはAI用の指示です。VS CodeのCodexへ送ってください。**

> このPCとUnityプロジェクトでGitを利用できる状態か確認してください。
>
> Gitが既にインストールされている場合は、再インストールせず、利用できることを確認してください。
>
> インストールされていない場合は、Windowsで現在推奨される公式のGitを導入する手順を案内してください。
>
> このUnityプロジェクトが既にGitリポジトリとして管理されているかも確認してください。
>
> 既存のリポジトリがある場合は初期化し直さないでください。
>
> まだGit管理されていない場合は、勝手に初期化せず、Git管理を開始するか私に確認してください。
>
> Git管理されている場合は、現在の変更状況を確認し、AIが今後変更した内容を`git diff`などで確認できる状態にしてください。
>
> Unityプロジェクトで不要な生成ファイルを大量にGit管理しないよう、既存の`.gitignore`も確認してください。
>
> Gitを導入しただけで、GitHubなどの外部サービスへリポジトリをPublishしたりPushしたりしないでください。ローカルGitだけで利用しても構いません。
>
> リモートリポジトリの作成、Publish、Pushが必要だと判断した場合も、勝手に実行せず、まずユーザーに確認してください。
>
> ユーザーが新しいリモートリポジトリへのPublishを希望した場合は、特別な指定がない限り**Privateリポジトリ**として作成する方法を案内してください。
>
> Publicリポジトリとして公開する場合は、公開対象のファイルやコードを確認したうえで、ユーザーから明示的な許可を得てください。
>
> 既にリモートリポジトリが設定されている場合は、その設定を勝手に変更したり、別のリモートへ置き換えたりしないでください。

</details>

## 8. 最後に接続確認

これで、

```text
Codex
   │
   ├── プロジェクトのC#を読む・編集する
   │
   ├── SkillでAnimator Code Pipelineの作業方法を知る
   │
   ├── Unity向けMCPでUnityを確認・操作する
   │
   └── Gitで変更を確認・戻す
   │
   ↓
Animator Code Pipeline
   ↓
NDMF / Modular Avatar
```

という環境になります。

最後にCodexへ簡単な確認をしてみます。

<details>
<summary>AI用指示</summary>

**これはAI用の指示です。VS CodeのCodexへ送ってください。**

> Animator Code Pipelineを使う準備が整っているか最終確認してください。
>
> Unity向けMCPが利用できることを確認し、現在Unityで選択されているGameObjectを取得してください。
>
> Animator Code PipelineのSkillが利用できることを確認してください。
>
> Animator Code Pipelineが現在のUnityプロジェクトへ導入されていることを確認してください。
>
> Gitが利用できる場合は、現在の変更状況も確認してください。
>
> 不足しているものがなければ、Animator Code PipelineでModuleを作成できる準備が完了したことを教えてください。
>
> この確認では、まだModuleやUnityの設定を変更しないでください。

</details>

## 9. AI / UnityMCP ワークフローへ

準備ができたら、[AI / UnityMCP ワークフロー](ai-workflow.md)へ進みます。

## 10. Visual Studio Codeのワークスペースを整える

Animator Code Pipelineを使うUnityプロジェクトをVisual Studio Codeで開き、作業用のワークスペースを整えます。

Visual Studio CodeではUnityプロジェクトのフォルダをそのまま開いて作業できますが、`.code-workspace`を用意すると、表示するファイルやワークスペース固有の設定を保存できます。

Unityプロジェクトのルートは、通常次のようなフォルダが並んでいる場所です。

```text
UnityProject
├── Assets
├── Packages
├── ProjectSettings
└── ...
```

`.code-workspace`と`.gitignore`は、既存の設定やGit管理範囲に影響するため、AIが勝手に作成・変更せず、内容を確認してからユーザーに許可を求めます。

特にGitについては、**最初にすべてをignoreし、Animator Code Pipelineで明示的に管理するソースだけをallowする方式**を使用します。

<details>
<summary>AI用指示</summary>

**これはAI用の指示です。VS CodeのCodexへ送ってください。**

> このAnimator Code Pipelineを使用するUnityプロジェクトを、Visual Studio Codeで作業しやすいワークスペースとして整えてください。
>/
> まず現在開いているフォルダとUnityプロジェクトの構成を確認し、`Assets`、`Packages`、`ProjectSettings`があるUnityプロジェクトのルートを特定してください。
>
> Unityプロジェクトのルート、既存の`.code-workspace`、`.gitignore`、Git管理状態を実際に確認してください。ファイルの場所や現在の設定を推測しないでください。
>
> `.code-workspace`または`.gitignore`を新規作成・変更する前に、必ず私に確認してください。
>
> 既存ファイルがある場合は上書きせず、現在の内容と変更案を先に説明してください。
>
> ## Visual Studio Code Workspace
>
> `.code-workspace`を作成する場合は、Unityプロジェクト全体をCodexから参照できる状態を維持しつつ、人が通常直接操作する必要のない生成物、キャッシュ、一時ファイルなどはVisual Studio CodeのExplorer上で非表示にしてください。
>
> `files.exclude`などに追加する項目は、現在のUnityプロジェクトの構成を実際に確認してから提案してください。
>
> ファイルをExplorer上で非表示にすることと、CodexやUnityから参照できなくすることを混同しないでください。AIやUnityの動作に必要なファイルを参照不能にしないでください。
>
> 複数のフォルダを含むMulti-root Workspaceが必要な明確な理由がなければ、Unityプロジェクトのルートだけを対象にしてください。
>
> `.code-workspace`を作成または変更する場合は、設定内容と保存場所を私に提示し、承認を得てから実行してください。
>
> ## Git
>
> Gitの管理範囲については、安全側に倒してください。
>
> **`.gitignore`は「すべてをignoreし、Animator Code PipelineでGit管理することを明示的に決めたソースだけをallowする」allowlist方式を必ず維持してください。一般的なUnity用`.gitignore`へ置き換えないでください。**
>
> この方針を変更したり、管理対象を広げたりする場合は、必ず事前に私へ確認してください。
>
> まず現在のプロジェクトを確認し、Animator Code Pipeline用としてGit管理する必要があるソースの候補だけを提示してください。
>
> どのファイルやディレクトリをallowするかは勝手に決めず、候補と理由を私に説明して、承認を得てください。
>
> allowlistを実装するときはGitのignoreルールを正しく扱い、許可対象へ到達するために必要な親ディレクトリだけを適切にallowしてください。
>
> Animator Code Pipelineのソースとして明示的に承認されていないファイルは、原則としてGit管理対象にしないでください。
>
> 特に次のようなデータを、確認なしでGit管理対象にしないでください。
>
> - 購入したAvatar、衣装、髪、アクセサリーなどの商品データ
> - BOOTH、Unity Asset Storeなどから取得した商品やAsset
> - 有料・無料を問わず、再配布条件を確認していない第三者Asset
> - Texture、Model、FBX、Prefab、Material、Animationなど、第三者から入手したデータ
> - ライセンスや出所が不明なファイル
> - ユーザーがGit管理すると明示していない既存のUnityプロジェクトデータ
>
> **有料の商品データや第三者の著作物がGitHubへ送信される状態には絶対にしないでください。**
>
> Private Repositoryであっても、安全だと判断して第三者の商品データをアップロードしないでください。Privateであることは、ライセンスや利用条件を無視してよい理由にはなりません。
>
> ファイルの所有者、ライセンス、出所、Git管理してよい範囲について少しでも不明な点がある場合は、推測せず私に確認してください。
>
> 一度確認したことを理由に他のファイルまで同じ扱いにせず、不明な対象が新しく見つかった場合は必要なだけ何度でも確認してください。
>
> `.gitignore`を作成・変更しただけで安全だと判断しないでください。
>
> 既にGitで追跡されているファイルには`.gitignore`が適用されないため、現在追跡されているファイルも必ず確認してください。
>
> `git status`や`git ls-files`などを利用して、現在追跡されているファイルと新たに追跡可能なファイルを確認してください。
>
> 最終的に、
>
> 1. 現在Gitで追跡されているファイル
> 2. 今後Gitへ追加可能なファイル
> 3. `.gitignore`によって除外されるファイル
>
> を確認し、Animator Code Pipeline用として私が明示的に承認したソース以外がGit管理対象になっていないことを検証してください。
>
> 既に不適切なファイルが追跡されている場合も、自動的に削除、Untrack、Commitなどを行わず、対象と問題点を私に提示して対応を確認してください。
>
> `.code-workspace`をGit管理するかどうかについても、現在のプロジェクト方針を確認し、勝手に追加しないでください。
>
> この作業ではCommit、Push、Publish、リモートリポジトリの作成を行わないでください。
>
> 最後に、作成または変更した`.code-workspace`と`.gitignore`の内容、およびGit管理対象となるファイルの概要を私に説明してください。

</details>

ワークスペースを開いた状態で、CodexからAnimator Code PipelineのソースとUnityプロジェクトを確認できれば準備完了です。

## 補足

このガイドではCodexを標準のAI環境として使用します。

OpenCodeなど別のAIコーディング環境を利用する場合でも、

```text
AIがプロジェクトを編集できる
        +
Unity向けMCPへ接続できる
        +
Animator Code Pipeline Skillを利用できる
```

状態にできれば、同じAIワークフローを利用できます。

