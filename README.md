# Animator Code Pipeline
<p align="center">
  <img src="./logo.svg" width="160" alt="Animator Code Pipeline">
</p>
Animator Code Pipelineは、VRChatアバターのAnimator機能をC#コードで作るためのUnityパッケージです。

- 衣装の切り替えや表情、BlendShape、オブジェクトの動作をコードで作成
- Animation Clip、Animator Controller、Animator Parameterをビルド時に自動生成
- 元のAnimator Controllerを壊さず、NDMFとModular Avatarで非破壊に統合
- AIによるコード生成やGitでの変更管理と相性がよい

## 導入

VCCまたは ALCOMから導入する場合は、
### [VPMリポジトリを追加](https://t4lly.github.io/vpm-repos/)
してパッケージをインストールしてください。

このREADMEと詳細ドキュメントをAIに読ませながら進めることもできます。

### AI導入用プロンプト

以下をAIにコピーして使えます。

```text
次のリポジトリを読み込んでください。
https://github.com/T4LLY/AnimatorCodePipeline

README.mdと
Packages/com.github.t4lly.animator-code-pipeline/Documentation~/guide.md
を読んでください。

私のUnityプロジェクトにAnimator Code Pipelineを導入したいので、
現在の環境を確認しながら、必要な設定を一つずつ案内してください。
私が操作する必要がある場合だけ、その都度わかりやすく説明してください。
```

## ドキュメント

基本的な使い方やAIを使ったワークフローは、[詳細ガイド](Packages/com.github.t4lly.animator-code-pipeline/Documentation~/guide.md)から始めてください。

AIを使ったワークフローはこちら[AIワークフロー](Packages/com.github.t4lly.animator-code-pipeline/Documentation~/ai-workflow.md)

## 動作確認環境

対応バージョンの詳細は [互換性](Packages/com.github.t4lly.animator-code-pipeline/Documentation~/compatibility.md) を参照してください。

## ライセンス

Animator Code PipelineはMIT Licenseで公開されています。

詳細は [LICENSE](LICENSE) を参照してください。

依存パッケージには、それぞれのライセンスが適用されます。