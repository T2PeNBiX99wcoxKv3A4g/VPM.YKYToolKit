# Changelog

## [1.1.1](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/1.1.0..1.1.1) - 2026-07-07

### 🚜 Refactor

- Replace transpilers with prefix methods in material patches and clean up unused methods in `Loader` - ([c142a8e](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/c142a8ee719f7d2ed918ed6259384b008f20b2df))

### ⚙️ Miscellaneous Tasks

- Bump package version to 1.1.1 in `package.json` - ([1fb2ff1](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/1fb2ff1a5819d8ede4da539bb81ff61d53db643a))

### Action

- Update `CHANGELOG.md` - ([efdb837](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/efdb8376f8d5a3ab56926107b7a229b4a890235d))


## [1.1.0](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/1.0.0..1.1.0) - 2026-07-07

### ⛰️  Features

- Integrate `MaterialShaderPatch` and `MaterialVariantPatch` execution in `Loader` under conditional compilation - ([6780822](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/6780822a3d58bdcd199514c1cb5957f03251d07b))
- Add `MaterialVariantPatch` with transpiler for `OnGUI` and conditional compilation support - ([6992c15](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/6992c1547140a0d3538c44461b24d3d07f6b1f30))
- Refactor `MaterialQueuePatch`, introduce `MaterialShaderPatch` with transpiler logic, and centralize `MaterialCheckTranspiler` in `Loader` - ([b3519b1](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/b3519b1e1bae752f9ecbf26f05f72365bcdef90a))
- Add `MaterialQueuePatch` with transpiler for `OnGUI` and patch loader system integration - ([3e532c6](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/3e532c638ac9c2559e20dc5e7598892b69a65dd6))

### 🚜 Refactor

- Change `MaterialCheckMethod` visibility from internal to private in `Loader` - ([307ac50](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/307ac508145bc0f77f4365b58d6a03cad45f0298))

### ⚙️ Miscellaneous Tasks

- Bump package version to 1.1.0 in `package.json` - ([1d215c1](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/1d215c17ff9d0dffb5a04ccae392049589d39a5c))
- Bump `io.github.ykysnk.utils` to version 1.3.1 in `vpm-manifest.json` - ([b2011b0](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/b2011b00b990934460a82993069e1eb97a30195f))
- Add `jp.lilxyzw.editortoolbox` version define to asmdef - ([3c2daca](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/3c2daca67eb0a24e6fcb5e812632a3287e0ec3f6))
- Remove unused `jp.lilxyzw.liltoon` dependency and update `jp.lilxyzw.editortoolbox` version in manifest files - ([771e841](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/771e841b808b27566a81eedde863494e549dca48))

### Action

- Update `CHANGELOG.md` - ([258f8d5](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/258f8d5d91017619526efbe4ec19ce104929196e))


## [1.0.0](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.20.0..1.0.0) - 2026-06-19

### ⚙️ Miscellaneous Tasks

- Release version 1.0.0 in `package.json` - ([1c7a4b3](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/1c7a4b3a4a055a27338de2dafb88e4a638353ca2))

### Action

- Update `CHANGELOG.md` - ([7cb43cd](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/7cb43cd5807fc2f83ae73ee102a2f9c307fbae4d))


## [0.20.0](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.19.0..0.20.0) - 2026-06-19

### ⛰️  Features

- Refine `ShouldExecute` to improve context comparison for menu commands using GameObject and Component switches - ([1c19d45](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/1c19d4532c30f702151451cb61c669d7cf078f9c))
- Add Undo operations for GameObject state changes in VRC and Unity constraint conversions - ([95b183b](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/95b183bd99c5e40a8d0977299c14630e9cc7f906))
- Add multiple GameObjects with constraints and components, including navigation mesh data adjustments and MonoBehaviour setups - ([40c95d9](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/40c95d9cc4b7559abdf97e011ef95a10089ec9a0))
- Add bidirectional conversion between Unity and VRC constraint types with extended support for nested data and menu commands - ([3bbeb03](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/3bbeb03c582aed7d648a90e5df0f66e5776a5ea1))
- Enhance `ConvertToVRCConstraint` with menu command filter, duplicate handling, and child traversal fixes - ([9c065c4](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/9c065c430cb0718789bc482266b64dc5a2c1b306))
- Add `Five` constant to `Util` and include new menu item for `ConvertToVRCConstraint` in GameObject menu - ([4286232](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/428623200c914bce5de42fa38e1d470d62fa1342))
- Add `ConvertToVRCConstraint` utility with support for multiple constraint types in context menu - ([74595f2](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/74595f235f5865ff99c5f02ace3406951e5a82ce))

### 🚜 Refactor

- Use `Util.StopwatchWaitElapsedMilliseconds` constant across all scripts for consistency - ([8ce3474](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/8ce34743fcc2f5787c049ab4e1d7cdbd143c8dfd))

### ⚙️ Miscellaneous Tasks

- Bump package version to 0.20.0 in `package.json` - ([1c42d42](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/1c42d422c2c1c28cc69b5447056e407b7ae922b7))
- Adjust default shader chunk size to 4MB in project settings - ([fbc7fc5](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/fbc7fc5249b3be2b488efa1635432295a4db1d45))
- Update asmdef references to include `VRC.SDKBase` and add `YKYTOOLKIT_VRCBASE` define constraint - ([74987d3](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/74987d393b69388a8f35e0d8a178bff2a3e8765a))
- Add `dev.onevr.vrworldtoolkit` package (v3.4.1) and update dependencies - ([0762ffa](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/0762ffa6066edb968efa3a0396de0cd11a3fa564))

### Action

- Update `CHANGELOG.md` - ([5c5e364](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/5c5e364238cc4b0aae1117faa961694435ef9a67))


## [0.19.0](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.18.2..0.19.0) - 2026-06-17

### ⛰️  Features

- Add new constants `One2` and `Twe2` in `Util` - ([443a146](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/443a14604ce9afaf5be626fddbf63829bf8467af))
- Add Copy/Paste Transform Tree functionality to YKYToolkit - ([792f104](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/792f1049353ca2e4f42e90cbbc15c889ebe86070))
- Optimize async operations in editor scripts - ([6545b9c](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/6545b9c98fa7b2fc19ed06e9027a6c4d204aa0e8))
- Improve async performance in editor scripts - ([6409f38](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/6409f385b19f8d6d46719e7949ef52268ceb4c07))

### 🐛 Bug Fixes

- Handle deleted assets in ImportWatcher - ([0620355](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/0620355a43b5dac44d4c02217c718246ebb654db))
- Reorder setter placement in ShowWarnWindow property - ([cb692c7](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/cb692c7a4a48770241452b971bda91f3b83448d7))
- Reorder property methods to ensure correct getter placement - ([37c14fc](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/37c14fcc316c075b19f94c42635a4f0ab9c139c5))

### 🚜 Refactor

- Replace `List` with `Dictionary` in `CopyTransformTree` for improved access efficiency - ([e840baa](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/e840baac8f83627817bd475b0c9b084d0f465ee1))
- Add priority to YKYToolkit menu items in CopyTransformTree - ([9d3799c](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/9d3799c43a857e3c737b03518175399506d38fe7))
- Add priority to context menu items in CopyAllComponents - ([d4fd07e](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/d4fd07e3cb38a4d764c915bc9edfd98f4b6f2a09))
- Add priority to YKYToolkit menu items in CopyAllComponents - ([40ebf0f](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/40ebf0f74a569bc89f2877799a9b2fab045987ae))
- Remove redundant blank lines in `TransformInputParser` for improved readability - ([5dbc719](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/5dbc71951ece4b40ae1f53099c503b9fc6494d45))
- Make `TransformInputParseResult` a readonly struct and simplify object initialization - ([49ea7a5](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/49ea7a57840daf5ffb3bbf29d3f6fb437a29aa47))
- Convert `ComponentData` from struct to class in `CopyAllComponents` - ([bddf0dd](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/bddf0dd3ce63e6c4a7fedc3d63f68291ec791aa6))

### ⚙️ Miscellaneous Tasks

- Bump package version to 0.19.0 - ([a0176db](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/a0176dbb552cb4647f6a379ca1d0ef9166384fab))
- Update Unity package dependencies and embedded modules - ([b909c61](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/b909c619f883d5e12c7c3f18354fdde5eb828735))
- Fix capitalization in README title - ([4859386](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/4859386953a45a63299fcbe517feafffbe56af31))

### Action

- Update `CHANGELOG.md` - ([5bd0a33](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/5bd0a3367c726edb63f0b09b75572110060bee76))

## New Contributors ❤️

* @github-actions[bot] made their first contribution

## [0.18.2](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.18.1..0.18.2) - 2026-05-07

### ⚙️ Miscellaneous Tasks

- Bump version to 0.18.2 and update changelog URL in package.json - ([f1d51a6](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/f1d51a6c3ff3d834490a3916008409eb38c88f6f))
- Update release workflow and package configuration - ([fd91e69](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/fd91e696705b46c7aaf699ccdf62c7c2d10db141))
- Add README.md for Yky ToolKit - ([6f667b0](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/6f667b022ad906de18ae4df43c346aafa3637bf4))
- Remove README.md - ([0955704](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/095570485e574ee13d4316c33a440d47b8cb7262))
- Update dependency versions in vpm-manifest.json - ([4ba3706](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/4ba3706743dd9e7e03ec2079d8b74c967e00da66))


## [0.18.1](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.18.0..0.18.1) - 2026-04-02

### 🐛 Bug Fixes

- Prevent CleanTransforms methods from running in Animation Mode - ([aa8fa3b](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/aa8fa3b5750dfa491eebdcd90287a40ed5351921))
- Prevent override clearing while in Animation Mode in EnhancedTransformInspector - ([ca4edfc](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/ca4edfc10f8f294549d14af1d0a82dbcac04d7b3))

### 🚜 Refactor

- Remove `DeltaAngle()` usage in EnhancedTransformInspector - ([64ed470](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/64ed470fa46aa16e85eb43e70b068ca10243efcd))

### ⚙️ Miscellaneous Tasks

- Bump version to 0.18.1 in package.json - ([364ed75](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/364ed75353b1264e966408288d6d9f0fc52884d2))


## [0.18.0](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.17.11..0.18.0) - 2026-02-25

### ⛰️  Features

- Enhance copy/paste functionality in EnhancedTransformInspector - ([7a2a471](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/7a2a4714dfecd65b95bf7f412cf7fb127e8e2496))
- Add copy and paste buttons to Vector3FieldExtra - ([aa13e45](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/aa13e456c14228e5b4efcd7855b30cb42b4ecf1a))

### 🚜 Refactor

- Update EnhancedTransformInspector to improve button configuration and add copy/paste support - ([5c6f3c8](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/5c6f3c8b85562dfcb2dc10b334ad2d10b32fa452))
- Replace button references with `IconButton` in EnhancedTransformInspector - ([64f6e45](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/64f6e45d29039576e24b1cbe46fbb97ab5a46a4f))
- Make `SetLinked` method private in Vector3FieldExtra - ([fd890d1](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/fd890d12bc43eff28ec1e848bad13a421263fc73))
- Replace `constrainProportionsScaleToggle` with `LinkButton` in EnhancedTransformInspector - ([809d5b7](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/809d5b76da2e51fe2cad24e4ce03aaf410d5822f))

### ⚙️ Miscellaneous Tasks

- Bump version to 0.18.0 in package.json - ([88eb199](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/88eb1993b7592344626d3b361f45d77f1a91a62e))
- Add copy and paste icons to Editor toolkit - ([6ee68c4](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/6ee68c458384ac5fe8379331eff4cd170fb0f472))
- Add com.unity.vectorgraphics 2.0.0-preview.25 and com.unity.2d.sprite 1.0.0 dependencies - ([adf0d44](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/adf0d44a681e997825f3da8c86a38ef6f9de3c16))


## [0.17.11](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.17.10..0.17.11) - 2026-02-18

### 🐛 Bug Fixes

- Try fix transform sync issue - ([370c8a5](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/370c8a5bfac1046a1a0da14e620d1f1ceb04654d))

### ⚙️ Miscellaneous Tasks

- Bump version to 0.17.11 in package.json - ([09132fa](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/09132fa6626ff0a3fd912b4306fbcf5ea4f6ff45))


## [0.17.10](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.17.9..0.17.10) - 2026-02-11

### 🐛 Bug Fixes

- Correct prefab override checks in EnhancedTransformInspector - ([67373c0](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/67373c0268e2bbe3efdbadf2ab3ed046e95ff5da))

### ⚙️ Miscellaneous Tasks

- Bump version to 0.17.10 in package.json - ([fe44cc1](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/fe44cc1a7483c9106094b8b93a8aab015c475335))


## [0.17.9](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.17.8..0.17.9) - 2026-02-08

### 🐛 Bug Fixes

- Remove redundant transform properties from Test.unity - ([d18c54d](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/d18c54de11a460d95e7e236ee561ae33f6c10194))
- Remove redundant transform properties from Item2.prefab - ([3f3ee8a](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/3f3ee8a4dceaac3401591ffd915f28aca7fefeb9))
- Add prefab override clearing logic and improve null checks in EnhancedTransformInspector - ([3b6d9ae](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/3b6d9aee94406d98f221b0aa9d770f94735fadc0))
- Reset Avatar_Base local position to origin - ([74e2b9a](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/74e2b9a17cd4a41699f1b8b741ae3dece4e655a5))

### ⚙️ Miscellaneous Tasks

- Bump version to 0.17.9 in package.json - ([d06a414](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/d06a4149e65b3726cf8a8732ef05a4626f723465))
- Bump io.github.ykysnk.utils to 0.44.2 in vpm-manifest.json - ([f74bf80](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/f74bf80662647aa4f80ad7b35ce89aa88a73c241))


## [0.17.8](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.17.7..0.17.8) - 2026-02-08

### 🐛 Bug Fixes

- Remove context menu logic to show default context menu - ([bcad560](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/bcad56069da7fab27ad5de4e0c522ab1678102bd))

### ⚙️ Miscellaneous Tasks

- Bump version to 0.17.8 in package.json - ([381944f](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/381944f9b717ba5ee64831a994f5d22baa18c439))


## [0.17.7](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.17.6..0.17.7) - 2026-02-08

### 🐛 Bug Fixes

- Add null checks in EnhancedTransformInspector to prevent potential null reference issues - ([b9888c2](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/b9888c2865ce2344b9e3b3942533e7d4024c5f61))

### ⚙️ Miscellaneous Tasks

- Bump version to 0.17.7 in package.json - ([a035a72](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/a035a729c5d17c48cf2cd3b54a7f4d9b800971b2))


## [0.17.6](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.17.5..0.17.6) - 2026-02-08

### 🐛 Bug Fixes

- Only apply vector3 when values is actually changed - ([9a64d29](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/9a64d29f134a31beec4bdfa71f02c0945323348f))

### 🧪 Testing

- Add new prefabs to Assets/Test folder - ([ece3638](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/ece36389de950e9ab857b1f69163dd3080de4c69))

### ⚙️ Miscellaneous Tasks

- Bump version to 0.17.6 in package.json - ([13cd67f](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/13cd67fc8d0a699aded07ae0f3a8438a3e75064c))
- Bump io.github.ykysnk.utils to 0.43.0 in vpm-manifest.json - ([b92b6b7](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/b92b6b7f42ed7d24938417df656ce69eeee87a19))
- Bump io.github.ykysnk.utils to 0.42.0 in vpm-manifest.json - ([f9b228f](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/f9b228fef2ce3a306177c5d31b76fe771e226ef6))


## [0.17.5](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.17.4..0.17.5) - 2026-02-07

### 🐛 Bug Fixes

- Update prefab transform modifications in Test.unity - ([a64a297](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/a64a2977e65b3228bcc7335b88c97cd2077950f3))
- Refine globalPositionField enablement logic in EnhancedTransformInspector - ([a6b9776](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/a6b97762a4b3fc29cbbc53105d01d2e4b4cf2572))
- Improve EnhancedTransformInspector handling for prefab contents - ([fcf04a2](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/fcf04a2054892f36eb875acef00d0f6531824688))

### 🧪 Testing

- Add new Cube prefab to Assets/Test folder - ([90e9f7f](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/90e9f7fecd933ed4c26a95b9829c39be34ac2798))

### ⚙️ Miscellaneous Tasks

- Bump version to 0.17.5 in package.json - ([7c883cc](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/7c883cccadeb28d7aa442fd9d0359c63e32cf101))


## [0.17.4](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.17.3..0.17.4) - 2026-02-07

### 🐛 Bug Fixes

- Adjust timeout for shader cache dialog display - ([a0dea59](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/a0dea594a10d3b9979219b40df9fc1c98154e908))
- Add timeout parameter to shader cache dialog display logic - ([939ec35](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/939ec35ffcb8c51e6702813082f7f9c09b5d6c06))

### ⚙️ Miscellaneous Tasks

- Bump version to 0.17.4 in package.json - ([c2fc60a](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/c2fc60ac16c0e1ccd6e5207ee71df89c2fdf91d9))
- Add support for Delete History Manager max records slider in PreferencesPage - ([32b49fb](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/32b49fb88e4f8c3071b3a71aaed992d51b0ecc85))
- Add translations for Delete History Manager in ja-JP and en-US locales - ([04b0087](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/04b008730704605f914eb87447b32ebc9dd5676d))
- Bump com.unity.memoryprofiler to 1.1.11 and io.github.ykysnk.utils to 0.41.1 - ([76a06dc](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/76a06dce3f3b7359c9f8e8e64bd3e2cc09bd3d1c))
- Bump io.github.ykysnk.utils to 0.41.0 in package.json - ([7cdba50](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/7cdba500cce796d4e27b51b69ef4a79773756aa1))
- Bump io.github.ykysnk.utils to 0.41.0 in vpm-manifest.json - ([0ad736f](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/0ad736f3ff14720a485c9f2fe381124a7a87316d))


## [0.17.3](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.17.2..0.17.3) - 2026-02-07

### 🐛 Bug Fixes

- Improve temp folder clearing logic and add "ZZZ_GeneratedAssets" to target list - ([4d05a8c](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/4d05a8c11e646d822380a4f5f2a46b3d623b59e0))

### ⚙️ Miscellaneous Tasks

- Bump version to 0.17.3 in package.json - ([2de8bb2](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/2de8bb2f44fcc87b8486ea54ec9bb5dc304ff8a4))
- Add liltoon package and update related settings - ([17ec936](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/17ec9362d8129b2964214a93bfa2499b9e7595d0))


## [0.17.2](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.17.1..0.17.2) - 2026-02-06

### 🐛 Bug Fixes

- Add basic imgui support for unity prefab override panel - ([69fe3ff](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/69fe3ff5530da0acb3bcec7a2480b16cfea55282))

### ⚙️ Miscellaneous Tasks

- Bump version to 0.17.2 in package.json - ([ed65794](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/ed65794ff444fd6b8c04c81d0f3a41b4226619c4))
- Add prefab instance and adjust local transform values in Test.unity - ([44deaee](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/44deaee75af22727400949f3d799ba0339c287c8))
- Add localization keys for IMGUI support message in EnhancedTransformInspector (en-US, ja-JP) - ([438838e](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/438838ebc5cec60a3414b69d5662649436609610))


## [0.17.1](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.17.0..0.17.1) - 2026-02-06

### 🐛 Bug Fixes

- Prevent null reference in hierarchyPathField schedule callback in EnhancedTransformInspector - ([906b4aa](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/906b4aad85de5fb19bc1f61263b824340c66199f))

### ⚙️ Miscellaneous Tasks

- Bump version to 0.17.1 in package.json - ([3e291bf](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/3e291bfdc01434094f9fe74039cb5bf1ba9b489b))


## [0.17.0](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.16.0..0.17.0) - 2026-02-06

### ⛰️  Features

- Add transform lock feature to EnhancedTransformInspector, including UI toggle and localization keys - ([3f2a380](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/3f2a38052b9e0bf8edb5f3e54b78d00a96377a4e))

### ⚙️ Miscellaneous Tasks

- Bump version to 0.17.0 in package.json - ([a3d89eb](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/a3d89eb345f23b3c5a6513f770da984f49428e35))
- Slight adjust to local rotation value in Test.unity for consistency - ([1da11b7](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/1da11b724911437d77b6868fbe446e61055f45b6))


## [0.16.0](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.15.1..0.16.0) - 2026-02-06

### ⛰️  Features

- Add EnhancedTransformDatabase and associated Editor implementation - ([b9f6256](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/b9f625648a4f9d6020a13b1433d3f66d3222798a))
- Add EnhancedTransformData and associated Editor UI - ([dd3ce38](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/dd3ce38b92f81fcd3bca46051d7e9e2a7f201b67))

### 🚜 Refactor

- Update EnhancedTransformInspector and related UXMLs for improved localization and code consistency - ([b145aa1](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/b145aa1d6ea1382a93f0a1d5379ce5f63430f4c1))
- Replace static decimal precision fields with dynamic values from EnhancedTransformDatabase - ([5577cf9](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/5577cf99f14a015115e406f462e1d2dddf3b2154))
- Persist decimal precision changes and save to EnhancedTransformDatabase - ([77e384c](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/77e384c27ebae9f6ef5276d9e0d33504bd2e5f47))
- Implement decimal precision controls for position, rotation, and scale fields in EnhancedTransformInspector - ([9a3ae30](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/9a3ae30a6c3ac5a847b074efdc1f22a4b7200daa))

### ⚙️ Miscellaneous Tasks

- Bump version to 0.16.0 in package.json - ([808a6de](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/808a6deda629954735e63bbc7b30d1e802d242b0))
- Add localization keys for clear import log and delete history dialogs in en-US and ja-JP - ([42f4b67](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/42f4b67db93d37efc0f11e3ce4ce1881ead25599))
- Adjust local position values in Test.unity for consistency - ([4bc08c0](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/4bc08c0229f86202ef0c1665ca213d6442b6887b))
- Add localization keys for decimal precision controls in EnhancedTransformInspector (en-US, ja-JP) - ([3bec1ad](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/3bec1add102de7a7ba5e7c518aae7c9f06e306ac))


## [0.15.1](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.15.0..0.15.1) - 2026-02-06

### 🚜 Refactor

- Replace DoubleField with SliderInt for duration and adjust related logic in ImportWatcher settings and PreferencesPage UI - ([2808ebf](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/2808ebf9064b58a421be906d84ade720d0352fb9))
- Add maxSessions configuration slider to ImportWatcher settings and update PreferencesPage UI - ([d19003a](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/d19003a8fd3eb37db0640f808e3268728d157947))
- Replace `EditorApplication.timeSinceStartup` with `DateTimeOffset.UtcNow` and adjust `HighlightInfo` logic in `ImportWatcher` - ([70d8bae](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/70d8bae6568874d3d0085c39dc6363c3e0fe0dd3))
- Implement scale constraint toggle and refactor scale editing logic in EnhancedTransformInspector - ([9dfd67b](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/9dfd67bf2e2b061a8d67b6f460663a1e1636c4dc))
- Rename editor keys and use PlayerPrefs for DeleteHistory; add configurable MaxRecords property - ([d7b3994](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/d7b399401961c2b09f6c13a025c4a4ee6525d39a))
- Rename editor keys and add configurable MaxSessions property in ImportHistoryManager - ([e3b5220](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/e3b522056e6689a5bca679e8b5638c268131845f))
- Replace DoubleField with Slider for duration and add maxSessions configuration in ImportWatcher settings window - ([89700dd](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/89700dd5e982387e8f9d9e81f1560e8e36312899))
- Add new localization key for max session count in English language asset - ([7926238](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/79262387da8a5edef6112952079059ab01ee5b5d))
- Add new localization key for max session count in Japanese language asset - ([90a7b9d](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/90a7b9d9c4c6844b554bfd920857d1451840e089))
- Remove unused `Depth` property and simplify `Node` initialization in `ImportSessionEditor` - ([f23f582](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/f23f582e79d11896a06ace8ca89d3a926776f65d))
- Extract tree-building logic to `BuildTree` and improve node management in `ImportSessionEditor` - ([c3e30c7](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/c3e30c72e02710ffba1d85be4b33d9987e88ceed))
- Suppress warnings in `UpmInstallerWindow` and add redundant property access for `isPackageListMakerExpanded` - ([697cbed](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/697cbeddea301325d13efabe12daeb77be90365c))

### ⚙️ Miscellaneous Tasks

- Bump package version to 0.15.1 in `package.json` - ([557f12a](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/557f12afbb2a4800efb4d2839bf34084eefbd865))

### Bump

- Update `com.unity.ide.visualstudio` to 2.0.27 and `com.unity.memoryprofiler` to 1.1.10 in manifest and lock files - ([92fdd68](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/92fdd683659af2679fbdb6a2a34cabebe7bce0a6))


## [0.15.0](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.14.0..0.15.0) - 2026-02-05

### ⛰️  Features

- Add `ImportHistoryManager` for managing import sessions and known GUIDs - ([92f535b](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/92f535b130b4f8cfaecd255247c896ba51d3b757))
- Add delete history tracking and management system with `DeleteHistoryWindow` and `DeleteHistoryManager` - ([cc6e594](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/cc6e594ddf310af512d2e73249e757376dfd7233))
- Bind undo/redo events to update rotation fields in `EnhancedTransformInspector` - ([6740bda](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/6740bdab6d4729837a532900d88e81b4742c489c))
- Add per-axis change detection for position, rotation, and scale fields in `EnhancedTransformInspector` - ([b5c9523](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/b5c9523bd67a3e2b9fba87777749ab4adaadf12a))
- Add editing state tracking for global position and lossy scale fields in `EnhancedTransformInspector` - ([2b8b561](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/2b8b5617c5337bdd1e80692ae4b7a8a50a4a5c66))

### 🚜 Refactor

- Reorder `CleanTransforms` calls and prepare toggled proportional scaling logic in `EnhancedTransformInspector` - ([149c6fe](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/149c6febbe8b4c195953a868e880c747d29b639f))
- Update asset deletion prompts in localization files to clarify undo limitations - ([145e75c](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/145e75cabc0a364f2ba07615d74263c0a7ae1bbd))
- Add `.uxml` and `.uss` file type color mappings in `ImportWatcherFileColor` for extended format support - ([5536554](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/55365545cd84e1214b0856b97f65a8ce78e312f6))
- Use `Try.Run` for cleaner exception handling in object and asset deletion methods - ([9e06eb1](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/9e06eb19fbe2f952296254dd9add195aaa38fe92))
- Simplify `DeleteSelectedAssetsAsync` by removing redundant braces and improving code clarity - ([79fad9e](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/79fad9ebffbc2df2599bb9d8af39cc659a5c9486))
- Update `ImportSessionEditor.uxml` with improved formatting and styling adjustments for enhanced UI consistency - ([e4d2b10](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/e4d2b103ae2d1cd30ab98ce3191aa9743cfb90d6))
- Clean up `ImportWatcherWindow.uxml` formatting and enhance `ImportSessionEditor` with folder icon logic - ([f66f7fb](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/f66f7fbca2fffb2dba8131cbfd4e8d392d4c074b))
- Replace `EditorGUIUtils.IconTexture` with `EditorGUIUtility.FindTexture` in `Vector3FieldExtra` for improved texture handling consistency - ([a80b686](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/a80b6867800d98f588116f5c49889afa6d8edae8))
- Integrate `TreeView` into UXML for `ImportSessionEditor`, streamline item creation and binding logic - ([a5d4496](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/a5d4496a4ab2d381402105a6c98749eeef91dadf))
- Implement hierarchical TreeView in `ImportSessionEditor` for better visualization of record paths and dynamic data binding - ([ca91ffb](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/ca91ffbd3ba8ee50f57223fc4b409cad7e635000))
- Remove `ImportRecordEditor` and related UXML/USS files, replace with `ImportRecordSegment` for streamlined UI and improved structure - ([dde580f](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/dde580f991b21f355d5d8d60f2ae031ed3a1325c))
- Remove unused `isFolder` field from `ImportHistoryManager.ImportRecord` struct - ([1cc9d94](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/1cc9d946578408fb81f6b8274e3254f6fc6f1612))
- Rename menu option for `Import Watcher Window` to `Import Watcher` for better clarity - ([c686fd6](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/c686fd65b76062bed882632926aff23e4a5ed4f0))
- Clean up UXML files for `DeleteHistoryWindow` and `ImportWatcherWindow` by improving formatting and style consistency - ([fd8546b](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/fd8546b2eb52c0d36e52459c2e8495f1e356d665))
- Replace inline UI element creation in `ImportSessionEditor` with UXML/USS, streamline record binding, and improve localization support - ([77baa73](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/77baa734a9e35f918556f5dfaeb5711ad1c3af57))
- Enhance UXML files for `ImportRecordEditor` and `ImportWatcherWindow`, clean up formatting, and update localization keys - ([ecdcdb4](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/ecdcdb4693d52975d0814ee4d67af2c0ee6ea63c))
- Streamline UI element handling in `ImportRecordEditor` and `ImportWatcherWindow`, fix asset path resolution, and clean up redundant code - ([ad6d62d](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/ad6d62d75c4ddfdc8947139d7dcb5df5867527a6))
- Extract `ImportWatcherSettingsWindow` from `ImportWatcherWindow` and streamline its functionality - ([0e135ef](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/0e135ef8ed3e4d572f699325b7cf25fe4d08cf61))
- Simplify `LoadInternal` logic in `ImportHistoryManager` with ternary operator - ([28b258c](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/28b258c1fb709b43cd72ed168d4247b10ca5a96c))
- Rename localization keys for consistency and update UI elements to use the new labels - ([8fce2dd](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/8fce2ddbbf2a8b62c006e951c8f6e671d81c352d))
- Add `name`, `path`, and `isFolder` properties to `ImportRecord`, update `ImportRecordEditor` to utilize them - ([7446dcf](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/7446dcfb63c53326ffa3eaa023f3a2f33fb003fa))
- Simplify `ImportRecordEditor` logic with guid-based properties, streamline `ImportHistoryManager`, and remove unused code - ([2041fbd](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/2041fbd47cd225bbd1e59d62eb7e82bff943ece0))
- Rename `ImportLogItem` to `ImportRecordEditor` and update associated files accordingly - ([73c0249](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/73c0249df8e61f28e6f35ec3f8c49de743a5c7f6))
- Use async dialogs for clear actions, update session fetching logic, and default foldout state - ([9968905](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/99689052da59c3a92f39dbcfb011ab3f9344ab4f))
- Simplify `ImportRecordEditor` logic, add double-click asset selection, and update UI handling - ([03440c6](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/03440c6a718ed629cf42d383e111d6df531b4b63))
- Replace synchronous dialogs with async versions and optimize visibility updates in editor windows - ([ccad3c4](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/ccad3c46a76f852c0303f79a47eb0a2d923bf749))
- Add import session logging and session tracking in `ImportWatcher` - ([143d66a](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/143d66ad3d81416522efe5b86298c736b7f5524c))
- Add import log management with refresh and clear actions in `ImportWatcherWindow` - ([cf99f97](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/cf99f976b8e6fd327612cddd35290e7e2e0e70b1))
- Add `ImportRecordEditor` with custom property drawer and UI components for `ImportRecord` - ([7793dc9](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/7793dc9da377560ac33fd3b2a2d7c1ca2cde1d9d))
- Add `ImportSessionEditor` with custom property drawer for `ImportSession` - ([8be39d6](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/8be39d67b6681e1da638d94578c4fe2376388391))
- Persist settings foldout state in `ImportWatcherWindow` using `EditorPrefs` - ([0a1e9ce](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/0a1e9ce09145211174f99fa93aba8b0628ec3bc9))
- Replace try-catch blocks with `Try.Run` for error handling in `EmptyFolderClear`, add non-Udon utility imports - ([b3e4e96](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/b3e4e963921a4350819486be27722eadaac2e1b8))
- Add localization for `label.import_watcher.settings` and wrap import watcher controls in a foldout container - ([ac1ca28](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/ac1ca28d6285d25beaa0430716c98bbd2aca8e4c))
- Remove redundant `UnregisterCancelCallback` call in `ForceClearTempFiles` - ([a3a4ef7](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/a3a4ef707753f925853eccb6e0f9429541a8df7c))
- Replace try-catch blocks with `Try.Run` for error handling in `DiscordEditorRPC` and `ForceClearTempFiles` - ([15e6be4](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/15e6be47575bc686adad2f22fc61c9f563cfead6))
- Add context menu for copying path and GUID in `DeleteHistoryWindow`, clean up unused bindings in `DeleteRecordEditor` - ([bbfd51f](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/bbfd51f220ef48715e47083d904f31288ce0db37))
- Improve localization support and add context menu actions in `DeleteHistoryWindow` - ([40b24d4](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/40b24d409b75f689b33f995ab19959ed8a8e4db8))
- Add `[PublicAPI]` attribute to `DeleteRecord` for improved external references - ([9f52259](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/9f5225938afbc5770216e96b529d4dbde9df3d33))
- Standardize localization keys in `DeleteHistoryWindow` and `DeleteRecordEditor`, remove redundant utility methods - ([70c49cd](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/70c49cd2d70e8d56c159b0af992ee62f63161d85))
- Add fallback option to `UILocalize` calls across editor windows for improved flexibility - ([c61adb5](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/c61adb592a7d9128eea55c88738426ad8f9bc583))
- Clean up `DeleteRecordEditor.uxml` by standardizing style tags and removing redundant bindings - ([07f4401](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/07f440173be060c2ba0b92cefd15af09f112021d))
- Simplify label binding and remove redundant `path` property in `DeleteRecordEditor` - ([4676687](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/467668724978e657b209d8789ac330e06fed13eb))
- Remove redundant `OnEnable` method from `DeleteHistoryWindow` to simplify code - ([b291e92](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/b291e923482ee576fb9aed65cf67baebcb6af7b6))
- Rename and replace `DeleteHistoryItem` with `DeleteRecordEditor`, adjust styles and bindings in `DeleteHistoryWindow` for improved clarity and functionality - ([38fc4ea](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/38fc4ea05f92e34cb4ce81f4e96701fbed058e02))
- Remove redundant lines and improve field initialization in `DeleteHistoryWindow` - ([844c348](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/844c348410413eede5352a5f8593000a2cb594f3))
- Extract undo/redo logic into dedicated methods and simplify field updates in `EnhancedTransformInspector` - ([84faa6e](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/84faa6e28e5283aea1138c951077777338c02fe2))
- Consolidate per-axis transform change logic with `ApplyToTargetsInChanged` in `EnhancedTransformInspector` - ([46b585a](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/46b585a7b932c8d7033b9db3a8f9b80068648e7e))

### 🎨 Styling

- Add `-unity-font-style: normal` to `.import-log-item__label` in `ImportRecordSegment.uss` for consistent text styling - ([3885c12](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/3885c12f1414eaab05750ffa6507d77e96e42a6e))
- Add `.empty-label` class in `ImportWatcherWindow.uss` for improved empty state styling - ([76f422c](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/76f422ccac99fd3def198a45a4378b2c0550735b))

### 🧪 Testing

- Add menu option to delete `ImportWatcher` import history using `PlayerPrefs` - ([5f64831](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/5f6483140665ac52675ce36fb2626f8d41ca4233))

### ⚙️ Miscellaneous Tasks

- Set custom application identifier for standalone builds in ProjectSettings - ([3c5d371](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/3c5d371f17d34f142b35848e7c3efd839bda59e1))

### Bump

- Update version to 0.15.0 and dependencies to latest compatible versions - ([cd52b39](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/cd52b39d64414b35dd22b57f9aae8fcba7c689ce))

### Update

- Add new localization keys for `label.delete_record.no_delete_history` in Japanese and English asset files - ([ba932c3](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/ba932c3c5707e73df349f186d6ca631da3d0f134))
- Add new localization keys for `label.import_watcher.no_import_log` in Japanese and English asset files - ([760d0a8](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/760d0a864c8c515b6fabd63ebc209914588c369a))


## [0.14.0](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.13.0..0.14.0) - 2026-02-04

### ⛰️  Features

- Add Preferences Page with localization support and new user-configurable settings - ([1a1de6a](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/1a1de6a433c350e78ca1930a4467bf2c9e64791b))
- Add localization keys for Import Watcher labels and update UXML to use localized labels - ([9c52ab6](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/9c52ab63d9f642858ce4102730dccbcc5d292052))
- Introduce `ImportWatcherFileColorEditor` with UXML and USS for customizable file color editing - ([15792bd](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/15792bdf8b351b06964cb7b883afbdec5a143400))
- Add automatic retry mechanism for `DiscordEditorRPC` initialization and reconnection handling - ([6c9f4ef](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/6c9f4ef2c3025a1132e2222a901aa57402948c27))
- Add activity start timestamp to `DiscordEditorRPC` initialization for improved presence tracking - ([7ca6446](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/7ca644620dc0cd53188c2ca9154c3066cf375eb3))
- Add headers and collection size display for `ListView` in `ImportWatcherWindow` UI layout - ([aa9906e](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/aa9906e16051b5e0d12096784bae8dca4c93fad0))
- Ensure unique file color entries in `ImportWatcherWindow` via `Distinct` method on load and destroy - ([31e9e16](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/31e9e1658e300a6a826078ebb621581686f9aab0))
- Implement equality and string representation methods in `ImportWatcherFileColor` for improved comparison and debugging - ([b5cea2a](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/b5cea2a05c457899ab5de8f86bae36af471f43bb))
- Add persistent file-specific highlight color management in `ImportWatcher` using `EditorPrefs` - ([3c341e4](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/3c341e4c25a8a0e15afaec60f3aa85b2577f409f))
- Add file-specific highlight color management in `ImportWatcherWindow` with `ListView` binding and persistence - ([4d2ebbe](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/4d2ebbe69447520e47f317a489bda7d7df5b7ae6))
- Add `ImportWatcherFileColor` for managing default file-specific highlight colors and update `ImportWatcher` to streamline color handling with extension mapping - ([031ff7a](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/031ff7a78950fd132f666dc4d6888d832d414151))
- Add duration customization to `ImportWatcher` and enable `editor-extension-mode` for UXML files - ([be0de86](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/be0de86713ed27de5fe119c31785905102d931ad))
- Add `ImportWatcherWindow` with support for customizable highlight color via `EditorPrefs` - ([54e9b68](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/54e9b682471276926575736d08b5a41d47d2703e))
- Add value binding and state transitions to `IconButton` with `INotifyValueChanged` implementation - ([ca011d1](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/ca011d10fb72bec50bdf5e327d20b8e1de9caf9e))

### 🐛 Bug Fixes

- Use `GetMainAssetTypeAtPath` instead of `LoadAssetAtPath` in `AssetExists` to improve accuracy - ([1ae7ec8](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/1ae7ec8c0270b47b06f5da851ea883860f78aa5e))

### 🚜 Refactor

- Consolidate `ImportWatcher` color logic, improve `PreferencesPage` bindings, and optimize file color management - ([fd897a9](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/fd897a9d2ce5597417d9c5d8dd109e0c47bdfc7b))
- Simplify `Initialize` method by removing redundant braces in `DiscordEditorRPC` - ([31a9396](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/31a939690d1d4a6c3fa1bd8f597dc962dca3358d))
- Replace hardcoded strings with constants and update property visibility for improved maintainability - ([7196c6c](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/7196c6c799f24912009738144db8824add16a02c))
- Reduce `RetryInterval` to 3 seconds and update log message in `DiscordEditorRPC` - ([ae6932f](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/ae6932f4ecd391d9296998a4054cf32031886076))
- Remove unused `DelayedInit` method and adjust `[PublicAPI]` attributes in `DiscordEditorRPC` - ([0aaef6b](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/0aaef6bf2a638170aca38fb01ceeae34e0e31f69))
- Update exception logging format in `DiscordEditorRPC` for consistency - ([5e80674](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/5e806746982e09d621c0a658d56b8b1dcd5c8e3b))
- Extract `ResetRetryTime` method and update retry logic in `DiscordEditorRPC` - ([6f06602](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/6f066020c7993556c1f845e5e7be0f282c2d4c51))
- Simplify null checks for `MethodsPtr` in `Filter` and `Sort` methods - ([7671c80](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/7671c807518c8bf061bd1c3e3f6054db0227b349))
- Remove unused logging in `SaveFileColorList` within `ImportWatcherWindow` - ([75d39f5](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/75d39f5662ecb3572fe761fd717ef95d9d5ccca1))
- Replace `Distinct` with `Rebuild` in `ImportWatcherWindow` for better list handling - ([240902c](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/240902ceefd2807e7beb4dab8d17767cab7f0c33))
- Use `Rebuild` extension method in `UpmInstallerWindow` to simplify list reprocessing - ([0326047](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/03260471ec460db330332a3461ea5bd1179038f8))
- Simplify `AddRange` calls in `ImportWatcher` and `UpmInstallerWindow` by removing unnecessary `.items` usage - ([9113676](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/911367665535d17aab05c375118336298da62248))
- Remove `UpmPackageListsWrapper` and replace its usage with `ListWrapper` for streamlined serialization handling - ([e150b50](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/e150b505385e9bf31e8afd616d169503f6235111))
- Replace `CopyData` struct with `ListWrapper<ComponentData>` for improved flexibility in serialization/deserialization - ([0972a29](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/0972a294152c373eec4af1d06671ab27b8e75990))
- Replace direct `JsonUtility` usage with `JsonUtils` for improved error handling and JSON safety - ([a3bb985](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/a3bb9856a7cf943b165effdfab8b5d99ad8be037))
- Wrap `EditorJsonUtility.FromJsonOverwrite` in `Try.Run` with error logging for safer component handling - ([6797470](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/679747083120ac639623079fa86df9aa1d49c7af))
- Replace direct `JsonUtility` calls with `JsonUtils` for safer JSON parsing - ([f1fedae](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/f1fedaee83e8cc24a1fe6eb06e5e6b37d1f6c2b2))
- Replace `TryGetImportHighlights` with `Load` using `JsonUtils` and remove outdated method - ([b3db32b](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/b3db32bb0f9e808bbc6b141bcdf4f5467083096d))

### 🧪 Testing

- Add menu option to delete persistent import highlight file color settings in `ImportWatcher` - ([046436e](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/046436e581a5dcb19f2a0f763d01acdfbef7bf66))
- Add `Test3` method for JSON serialization/deserialization demo with `JsonUtils` - ([c09a387](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/c09a387a2d797bffc5a8d53ea757b3873b615705))

### ⚙️ Miscellaneous Tasks

- Bump package version to 0.14.0 in `package.json` - ([ffe12d1](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/ffe12d14316a00583cb95db3e2a94dabbfa3a74c))
- Update `MenuItem` attributes in `DiscordEditorRPC` to include `Util.Four` for consistent menu ordering - ([dad0a1b](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/dad0a1b41483eaa21c870446c6472b20015b73e4))
- Add `Util.Four` constant for expanded menu ordering usage - ([00f47d0](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/00f47d0faeaeb81e08e133155687726c9f2b055c))
- Update `MenuItem` attributes to use `Util.Three` for consistent menu ordering and modify window title icon in `ImportWatcherWindow` - ([4d0d54f](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/4d0d54ff0545ca639a019206258fb48e641d07e4))
- Update TODO comment in `ImportWatcher` to remove outdated "Menu for settings" task - ([98d290e](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/98d290eec0c60e4753fcb6cd0fbdf2c2e444de8d))
- Bump `io.github.ykysnk.utils` to v0.40.1 in `vpm-manifest.json` - ([f93bb40](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/f93bb40df3231a4a32f48e41d8692842ce798b93))
- Bump `io.github.ykysnk.utils` to v0.39.0 in `vpm-manifest.json` - ([d939a32](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/d939a326288679614979d393ed484deeeab20cbd))
- Bump `io.github.ykysnk.utils` dependency to v0.38.0 in `package.json` - ([3a960fd](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/3a960fd5d0049a1c30370e1de9c0c5252b697277))
- Upgrade `io.github.ykysnk.utils` to v0.38.0 in `vpm-manifest.json` - ([6b8b812](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/6b8b812dd85ede8bed21cd8907f4035263980afd))
- Update log message in `ForceClearTempFiles` for clarity - ([a1ae691](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/a1ae69120c3e3a1d14f1f4cce4700f11b54ffaec))
- Add TODO comment placeholder in `ImportWatcher.cs` for time modification - ([d08159a](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/d08159aea81f289dd37569bc7e5308f8cecc75da))
- Upgrade `io.github.ykysnk.utils` to v0.36.0 in `vpm-manifest.json` - ([f064864](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/f06486496a509e14e48b585c216cf66ccf7cb4f9))
- Bump `io.github.ykysnk.utils` to v0.34.0 in `vpm-manifest.json` - ([dbee405](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/dbee405ba1f22fa5150cb0f96089d33ccf8081a9))
- Remove unnecessary blank line in `ImportWatcher.cs` - ([26e4073](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/26e4073d69dbfb475dba170b5dbe2dc02ee90183))
- Add localization keys for enabling/disabling constrained proportions and update tooltips in `Vector3FieldExtra` - ([dc13d94](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/dc13d949378f36c0d9434ae960ee9a00b4d17e76))
- Enable constrained proportions scale in `Test.unity` - ([c56f0b6](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/c56f0b6d8b30d99ea4ef4e5c78fa03d840b34e40))
- Fix JSON formatting in `io.github.ykysnk.yky-toolkit.Editor.asmdef` file - ([806caa0](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/806caa0cdf8381da0f91704e6781e3adb12a46cc))
- Add position, rotation, and scale localization keys with tooltips in en-US.asset - ([48d19bf](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/48d19bf9821726ff33fd5c13b204d8d08ca33b4e))
- Clean up UXML formatting in `EnhancedTransformInspector` for consistent style usage and tooltip updates - ([b5cfbac](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/b5cfbacd9554a1ded266540ca8f1eacf24465d46))
- Bump localization package to v0.9.2 and maintain utils dependency at >=0.33.2 - ([2d2c7ef](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/2d2c7ef6e121472ba6c7e3446fcf91ff5f701a4f))
- Clean up UXML formatting in `EnhancedTransformInspector` for consistent style usage - ([5ee4f56](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/5ee4f5657afeac9601b2a3ea0e1455b8dae4c048))
- Add tooltip to link button in Vector3FieldExtra for constrained proportions - ([818e58e](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/818e58ec08b56fd0c386d7aadf3333ee4cb848a7))
- Bump localization package to v0.9.1 and update utils dependency to >=0.33.2 - ([574f485](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/574f4856a95585906e30f0ba3187ecefc2ac71fb))
- Update localization package to v0.9.0 and clean up UXML formatting in `EnhancedTransformInspector` for consistency - ([f151b46](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/f151b46afbd13a0e0f940942b2ae47bd8267478f))
- Add `dev.sakurayuki.uniasset` package (v1.3.0) and bump localization package to v0.9.0 - ([24ded32](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/24ded32537e170d2c4fc932d6727854862a0175c))
- Add scale linking functionality and remove unused bounds size handling in `EnhancedTransformInspector` - ([881a093](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/881a093580dfa8ee880356098b9d67f832cd1ca1))
- Refactor icon usage by centralizing Vector3FieldExtra styles and updating EnhancedTransformInspector - ([c524f7f](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/c524f7fa963ee759008ccc00d17cf130cde6b8d4))
- Bump localization package to 0.8.0 - ([51e4d04](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/51e4d04f459e045cea42f646cc2a57a07f82da54))
- Add tooltip and alignment styles, clean up UXML formatting for consistency - ([3d2eda0](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/3d2eda04167322bf4fcbed64b902533ee35b03dd))
- Update Test scene object rotations and local position values for consistency - ([d796162](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/d796162042e6dbf19d0e32febd3ce3310def15d3))

### Faet

- Add `ImportWatcher` utility to track and highlight recently imported or moved assets in the Project window - ([c7f0bfa](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/c7f0bfac12f2e163a2b968df05bf92df381e5f07))


## [0.13.0](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.12.0..0.13.0) - 2026-02-01

### ⛰️  Features

- Extend `EnhancedTransformInspector` with `Vector3FieldExtra` and `IconButton`, add reset/random handlers and rotation field edits - ([dafff69](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/dafff690f5a935ca61d9fa889f267434f8c704e3))
- Add `Vector3FieldExtra` with extended functionality and customizable tooltips to YKY Toolkit UI elements - ([f1a9200](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/f1a92007835c8d769a11b32fe27638cec449a64e))
- Replace standard fields in Enhanced Transform Inspector with `Vector3FieldExtra` and integrate `IconButton` for improved functionality - ([c4da020](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/c4da020ae6098902ced89abed142478de793902a))
- Add `IconButton` UIElement to YKY Toolkit with support for click events and extra animations - ([5c325b7](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/5c325b75362e3dccaca9f9e384e7c5e519d386d8))
- Add new `Cube (2)` GameObject with box collider, mesh renderer, and mesh filter to Test scene, including hierarchical relationship updates - ([0d4dad5](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/0d4dad58f1d5793242c58be76910f4bb61e0b3db))

### ⚙️ Miscellaneous Tasks

- Bump package version to 0.13.0 - ([494c640](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/494c640aadad9927fe09ad02d7905eb9feec6dc2))
- Clean up unused comments in `EnhancedTransformInspector`, update Test scene object rotation values - ([fc34fee](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/fc34feea766a360a3bed63818f9775dc37eeff1b))
- Update solution settings to include user dictionary entry for `rotatetool` - ([06ef9d9](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/06ef9d99b1aa6a744d8629e4feb11e157a0457d2))
- Update package dependencies, add `Mirror Tool` and `Editor Core` packages - ([4f28a7a](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/4f28a7a7916a962a20fe79529330b672c5d8da8d))


## [0.12.0](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.11.0..0.12.0) - 2026-01-29

### ⛰️  Features

- Add `Clean` utilities for world transforms, improve field precision, and update Test scene with new GameObject - ([77a89e3](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/77a89e373b8c95f908357ee9a1c787ce2ee54250))
- Localize ContextualMenu actions in Enhanced Transform Inspector and add new GameObjects to Test scene - ([6bb27d7](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/6bb27d72187ec42b35e9d0aafa1e539fa915b0c1))
- Add Help section to Enhanced Transform Inspector with expandable foldout and localized tooltips - ([7b6a82c](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/7b6a82c32b616a7a98e1b855e12441bafae33479))
- Implement Distance mode logic for Enhanced Transform Inspector, adding sorting by distance and index-based value calculation - ([47f46aa](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/47f46aa37d86955e46c4fbc0b7021e953fd8b9dd))
- Add new TransformInput modes (Clamp, Mirror, Step, PingPong, Angle, Noise) to Enhanced Transform Inspector and extend parsing logic - ([38104d2](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/38104d25b7d47938783fb0c6ef211b2187aa9f91))
- Extend TransformInputParser with `TryParseThree` method and comment categorization, update Unity scene rotation precision - ([e6dbf08](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/e6dbf08865fc0fad92a6b8c5a0b96316f09ac039))
- Add Division mode to Enhanced Transform Inspector and improve input parsing logic - ([7a04278](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/7a0427816a0efd733df70d2bb761cb306eda4fef))
- Improve Enhanced Transform Inspector with `Clean` utility for rotation fields and UXML updates for binding consistency - ([94b5729](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/94b5729751ee9b9712e5a58857cd9e9ae0e185f4))
- Refactor ground alignment logic with `AlignToGround` method and `AlignMode` support - ([5509b58](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/5509b5844ca15138d74d761b66f2409a93c2881b))
- Add `AlignMode` enum and save functionality to Enhanced Transform Inspector - ([8b3a125](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/8b3a12519ab50d845ab527b0f53bc4d4e279d144))
- Improve Enhanced Transform Inspector with precise callbacks and rotation editing fixes - ([69eaaee](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/69eaaee27df6ced65194145ec67d59d5c784bdde))
- Add "Clear Parent" button to Enhanced Transform Inspector for quickly detaching parent objects - ([573bc5c](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/573bc5cc396d244c93712d1e9a83e6838628c31e))
- Update translations for Enhanced Transform Inspector with new string keys and adjustments - ([97277e3](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/97277e3a072dd1834872a686f6e8e0af9ba5a9ed))
- Enhance axis input handling in Enhanced Transform Inspector with improved parsing feedback and new modes - ([08f76b0](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/08f76b0c896297b2acee9ef0d68f96ba6d68696e))
- Add multiple GameObjects with defined meshes, colliders, and transformations to Unity scene - ([bc5c09f](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/bc5c09ffdbc972ffb375801ffeaf49dd3ae6bb45))
- Add Enhanced Transform Inspector with advanced transformation tools and utilities - ([1dfd422](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/1dfd4225dcfecfe52f3fb07b6c0572bb4ad91e3f))
- Add `PRSData` struct for managing position, rotation, and scale data in `yky-toolkit` - ([671923a](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/671923acb1f6565a02cdbdbec806b692a88d0515))
- Add `TransformInputParser` for parsing transformation input strings in `yky-toolkit` - ([fde9d0b](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/fde9d0bc55e1640684239f9005b3eac54c3896f8))
- Localize Enhanced Transform Inspector with new string keys and translations - ([89210f5](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/89210f57b7f1ce84bc467b3f952899f7b0ff5e3d))
- Add USS styles for Enhanced Transform Inspector in `yky-toolkit` - ([7159691](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/71596916a52c4e131017e180c90bf794380bbbb4))
- Add multiple GameObjects with meshes and colliders to the Unity scene - ([01df8f8](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/01df8f8cc3a3446bd609cfcf2fe38d7677dc8cf0))
- Add "Discord SDK Restart" menu option and clean up unused constants - ([9587ce6](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/9587ce6593eb42f70413165f992bc0ca2a94bef1))

### 🐛 Bug Fixes

- Remove duplicate Distance mode logic in Enhanced Transform Inspector - ([323a767](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/323a76759d33e7b7c042d591bee6e50ae003f4b1))
- Resolve incorrectly handled L input in Enhanced Transform Inspector rotation field update - ([24edc99](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/24edc99f32b13d3b9b9e79e7b3299ac24b976642))

### ⚙️ Miscellaneous Tasks

- Bump package version to 0.12.0 and update dependencies to latest versions - ([a1d73ee](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/a1d73ee88fd05232821b867f2781d0b168bdf52e))
- Clean up UXML and USS files for Enhanced Transform Inspector with consistent formatting adjustments - ([c2ac3c0](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/c2ac3c00e2922b980c9130389408e5648538e528))
- Bump `io.github.ykysnk.localization` package version to 0.6.0 - ([41486ac](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/41486acb15fabbba1b4e472d65d3a32117dac995))
- Bump `io.github.ykysnk.utils` package version to 0.33.2 - ([afcc487](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/afcc48731fc7b2b871b4aab04c8a29aa164b39ce))


## [0.11.0](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.10.0..0.11.0) - 2026-01-27

### ⛰️  Features

- Localize "Clear Empty Folder" and "Clear Shader Cache" dialogs with new string keys and translations - ([a3a4024](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/a3a4024cb20fe0ad0bf0985186fa7959fce039b6))
- Add Editor features for Unity Discord Rich Presence and activity detection - ([5c5e327](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/5c5e3276ea818f9b033f5b137226a249529a4939))
- Add comprehensive Discord core implementation in `yky-toolkit` for SDK support - ([6893d01](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/6893d01f0ffbe730fa9b41755917ed492d73db4d))
- Integrate Discord game SDK with plugin binaries, constants, and configuration files - ([72ee8ec](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/72ee8ec65b911d32fab5044a953f5c8cab51b741))

### 🧪 Testing

- Add Discord RPC test menu items for activity management and logging - ([e9e3c70](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/e9e3c70b0da5d72ef5e33b0a545098b852ea2837))

### ⚙️ Miscellaneous Tasks

- Bump package version to 0.11.0 and add `io.github.ykysnk.localization` dependency - ([2f41663](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/2f4166303a5edfa58266eb403e1e73f5b34e369b))
- Add `io.github.ykysnk.yky-toolkit` to asmdef references for Editor - ([ea8d5cb](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/ea8d5cb42c822d5d2ff856ee18109e31ed43000f))


## [0.10.0](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.9.1..0.10.0) - 2026-01-27

### ⛰️  Features

- Update deletion dialog with localization support and new string keys for improved UX - ([85bb6bd](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/85bb6bd2b588a9a9c4ca571dcb9b5ec825306a30))
- Revamp `UpmInstallerWindow` with additional package list management features, UXML updates, and localization improvements - ([ad675d8](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/ad675d8d3ddda3fa16d9945d0eb7245aedc0f893))
- Add `UpmPackageListsWrapper` and `UpmPackageListWrapper` classes for structured UPM package management in the editor - ([d8894ad](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/d8894ad53db54023941308616a50c983004f9fd1))
- Add `UpmPackageListWrapperEditor` with UXML, USS, and bindings for enhanced UPM package management in the editor - ([cc3adba](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/cc3adbafa03e38ffdf17f55bb09d475db94df547))
- Update UXML files and localization assets to support enhanced localization strings and structure - ([60e04cf](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/60e04cfe9745283ae4cdafe94a820c62aa0322e8))
- Add localization assets for ja-JP and en-US languages in editor - ([c464050](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/c4640503e0a57930e983605ce1a32e7119ea3e68))
- Integrate internal localization support in editor components and add `InternalLocalizationExtensions` - ([c2ef2f4](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/c2ef2f4240cbba1c8a8f613a29493be341ed619b))
- Add `UpmInstallerPackage` class and editor support for managing UPM packages - ([429888e](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/429888e55fd4a5c7ba680e37d1f397df18b881fd))

### 🚜 Refactor

- Replace `UniTask.NextFrame` with `UniTask.DelayFrame(10)` across multiple scripts for consistency - ([01a0c1b](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/01a0c1be9fb3cbf9e22532c74520f7dab19fe0d4))

### ⚙️ Miscellaneous Tasks

- Bump package version to 0.10.0 in `package.json` - ([238dae3](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/238dae3fe126c81ae27e00a9b74c4a4d55461615))
- Remove obsolete `LabelWithLabel.uxml` and related meta file - ([8e0740d](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/8e0740ddc2e631ffb859367839c8c814710a97ba))
- Bump com.unity.ide.rider to 3.0.39 and io.github.ykysnk.utils to 0.32.0 - ([4905c62](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/4905c62fdaf1fb63760530756928c1c128a9c171))


## [0.9.1](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.9.0..0.9.1) - 2026-01-19

### 🐛 Bug Fixes

- Correct typo in delete confirmation dialog text in NewDelete script - ([220b789](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/220b7895a893e4f03b5e0d5bcbd8ffbaa9a6c26d))

### 🚜 Refactor

- Rename `ShowWarnWindow` to `AllowAsyncCompilation` in ShaderAsync for clarity - ([2e0d8ca](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/2e0d8ca138b309e041bac83f50a853beec657e77))

### ⚙️ Miscellaneous Tasks

- Bump package version to 0.9.1 - ([824f80e](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/824f80e503f3f85bf3f4afe6e6a0f1f5adebee37))
- Add io.github.ykysnk.localization package 0.5.4 with dependencies - ([1c97807](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/1c978074c20e0dfdd57c8e60679b1cc4d928b70a))


## [0.9.0](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.8.0..0.9.0) - 2026-01-16

### ⛰️  Features

- Add "Force Clear Temp Files" utility with async support for cleaning temp folders via menu - ([365c4cd](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/365c4cd72ce2bf191c2eae46f66bec69694be6f2))

### 🚜 Refactor

- Remove redundant AssetDatabase.Refresh call from ForceClearTempFiles utility - ([c34913c](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/c34913cdc3f1173de59d56dcc61b7b67cd35b93c))

### ⚙️ Miscellaneous Tasks

- Bump package version to 0.9.0 - ([f27efd1](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/f27efd18e212f32f60b2d111508d4d4b2ed40038))
- Bump io.github.ykysnk.utils to 0.30.2 - ([6245104](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/6245104c8729f5a967b61e5fa31efd4c0fc0275b))


## [0.8.0](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.7.1..0.8.0) - 2026-01-16

### ⛰️  Features

- Add ShaderAsync utility to toggle async shader compilation via menu - ([0b2ab56](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/0b2ab560716add9346a090b4aead7821801cd3a7))

### 🚜 Refactor

- Replace menu item priority constants with unified calculation logic in Util for better consistency - ([5f151aa](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/5f151aae0040029db10a587adb26ca99af241490))

### ⚙️ Miscellaneous Tasks

- Bump package version to 0.8.0 - ([7bc5e1b](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/7bc5e1b293bfa8060ccaa47624b37296d3062f0d))


## [0.7.1](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.7.0..0.7.1) - 2026-01-16

### 🚜 Refactor

- Simplify menu item paths in NewDelete script using constants for improved maintainability - ([5864ba3](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/5864ba3683ff029f7ed126dabc06a63fd4506471))

### ⚙️ Miscellaneous Tasks

- Bump package version to 0.7.1 - ([20ef3f6](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/20ef3f6a3061efe60e89da48cba668d02f814ed8))


## [0.7.0](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.6.1..0.7.0) - 2026-01-16

### ⛰️  Features

- Add NewDelete script for handling object and asset deletion with progress tracking and async support - ([241cafe](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/241cafe93d4248fbe5352d10623e91a2817fcf2e))

### ⚙️ Miscellaneous Tasks

- Bump package version to 0.7.0 and update io.github.ykysnk.utils dependency version - ([8c9683c](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/8c9683cd7de8fe79fa31a375c348ec081084bf7c))
- Add DeleteSelectedPriority constant to Util for menu item prioritization - ([9bc3492](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/9bc349291ed62d6134f1af44f1062b1fc19e8995))
- Update dialog button text in EmptyFolderClear for clarity - ([ac3c15f](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/ac3c15f603ce0b3ac58e4d6ebabf4d0fad79258c))
- Add Oculus XR settings to EditorBuildSettings asset - ([feb75cb](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/feb75cba3af7dbd6e2316baecaff2936d0fd1a21))
- Update dialogs and progress tracking in EmptyFolderClear and ClearShaderCache for improved user interaction and cancellation handling - ([b12938d](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/b12938d96e65425d5773fbb42ccdcdbc24c480ab))
- Replace UniTask.Delay with UniTask.NextFrame for improved async operation consistency - ([56a4d59](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/56a4d59f61a93ecf503b1de35cca9d3d17f04ef1))
- Remove unused GameObjects and components from Test.unity asset for cleanup and optimization - ([71270ba](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/71270bacd8907b860fbd4f27ba7aad8c515ae156))
- Refactor UnityResourceMonitor rows into individual classes for better modularity and maintainability - ([00fbdf8](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/00fbdf8143fb95be08b15d529da8f84485efc0e0))
- Update PackageManagerSettings with new registry and advanced settings fields - ([a6a4bea](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/a6a4beaf3f55702a11b2168b7807896640c40178))


## [0.6.1](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.6.0..0.6.1) - 2026-01-13

### 🧪 Testing

- Add script to generate nested empty folders in the Unity editor - ([6fff53f](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/6fff53f41b9205d46c9b99dfd6ce9f20304302aa))

### ⚙️ Miscellaneous Tasks

- Bump YKY Toolkit to 0.6.1 - ([59890a2](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/59890a2cf859bb6146722d9d9537059a09456df2))
- Add TODO for converting package string to custom class in UpmInstallerWindow - ([a1c1bc9](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/a1c1bc913c5aa1d5d798d7b092de271fa305189a))
- Add async paste operations with transform support in CopyAllComponents using UniTask - ([fa80cff](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/fa80cff5dc7b5b1d44d01e1d1bfec3cde94b4068))
- Add new GameObjects with components including BoxColliders, MeshRenderers, and Transforms in Test.unity asset - ([bafaacf](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/bafaacfa752d3cf61aaaeb990ec4c5b325c6c677))
- Refactor asset backup, folder clearing, and object tagging to use UniTask for async operations - ([005ae6f](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/005ae6f4a6c829dcbeb765d062738c7013542677))
- Bump io.github.ykysnk.utils to 0.30.1 - ([66f536f](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/66f536f91e83bc5760a5f31edc5fc4f6d8dc436f))
- Refactor folder and shader cache clearing to use async, progress tracking, and improved dialogs - ([53d1284](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/53d12843f1aad16d0ebb9ef26155de9275f9b6a1))
- Bump io.github.ykysnk.utils to 0.30.0 - ([364ea78](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/364ea78e47eb03f790b911a3d11205e3c078086e))
- Add async empty folder clearing with UniTask and progress tracking - ([4a68400](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/4a684003eeefa7397f200efbbe5af97e266c9e38))
- Add UniTask dependency to YKY Toolkit editor assembly definition - ([553c58c](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/553c58c2d721c8de9f57973f59791d857ce5dee7))
- Update UPM Installer Window title icon from "CloudConnect" to "package manager" - ([72a76f4](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/72a76f462bf242d2ef9e5efc90bbd07b75a8b89b))


## [0.6.0](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.5.1..0.6.0) - 2026-01-13

### ⛰️  Features

- Add UPM Installer Window with UI implementation and package management actions - ([2ee916e](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/2ee916e707040360d5fd55eeace48e92dbb83667))

### ⚙️ Miscellaneous Tasks

- Bump YKY Toolkit version to 0.6.0 - ([72aae4b](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/72aae4b1b70df84df8c0025a2af0dee9f09fc3ac))
- Remove `UpmMemoryProfilerInstaller` and associated meta file - ([a43e3b8](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/a43e3b8b93c5be79bd18c7e62a0f08c7370d9cdc))
- Update Unity package dependencies and add Build Report Inspector package - ([051c621](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/051c62139afa78a5252a46a1ac000a44dc0dbb25))
- Adjust UnityResourceMonitor styles and add conditional reload check - ([ebf5686](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/ebf5686de2452513324acca5fed013a19f75ff04))
- Bump io.github.ykysnk.utils to 0.29.2 - ([263cbe0](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/263cbe053eb8364db820ea3fb8dd1930c6d7ea89))
- Refactor UnityResourceMonitor to simplify GPU handling and add reload functionality - ([3599bae](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/3599bae9bcbc0b6f4216b32bdef53999d586da8b))
- Bump io.github.ykysnk.utils to 0.29.0 - ([31f9c55](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/31f9c55168e6bdcffe8ed83aab47d62574f995da))
- Refactor `UpmMemoryProfilerInstaller` to use `UpmInstaller.Install` method - ([a438c1c](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/a438c1ccfa48c5623c41a98741b08aff4a753ef9))
- Remove UpmInstaller and related extensions - ([440c22f](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/440c22fcab5eb7a15cf34c6c7246cbc5678a7ac1))
- Bump io.github.ykysnk.utils to 0.27.0 - ([727e03e](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/727e03ee888a5531479b279af4638506d726db08))


## [0.5.1](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.4.0..0.5.1) - 2026-01-11

### ⛰️  Features

- Add icon to Unity Resource Monitor window title - ([6ac6ff7](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/6ac6ff7e6a11fd1a2418d37e2545bd7c9c5d1a56))
- Add UPM installation utilities and internal tools, tighten access modifiers for UnityResourceMonitor - ([2b1529a](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/2b1529ae2d7f6350913184d7d48017099ae02597))

### ⚙️ Miscellaneous Tasks

- Bump version to 0.5.1 - ([c0d47b8](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/c0d47b8617498cd3684d13f030e1d948533c38d8))
- Remove UpmInstaller because  windows is dumb - ([5bf0ac7](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/5bf0ac754cfc41a14475205dd249bb4e408d7e1d))
- Bump version to 0.5.0 - ([4d6ed45](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/4d6ed45614ae36145fbfbf24c4f6145e400bed37))
- Bump version to 0.4.1 and clean up unnecessary dependencies - ([a308243](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/a308243eaf169c2116c153f13d37d3a5b8132fac))
- Remove unused dependency reference from packages-lock.json - ([e04b868](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/e04b868ac8cd0d2deab49ebb0a5c7978aeb94b04))
- Update UnitypackageImporter script binding and remove EditorClassIdentifier - ([6d40104](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/6d401041d0a281bf23620d90ff7968a33789b5a7))

### Chroe

- Recreate UpmInstaller.cs - ([de8dcc1](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/de8dcc14ddc82f0d6551ced2fc69846a6edd872a))


## [0.4.0](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.3.1..0.4.0) - 2026-01-11

### ⛰️  Features

- Enhance Unity Resource Monitor with CPU/GPU usage tracking and ID-based row management - ([5e178fc](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/5e178fc1d8b971b1da42b16f5f77c957f14b8e26))
- Introduce Unity Resource Monitor with UI for tracking editor resource usage and profiling - ([7c17cfe](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/7c17cfe0a8ca11c6d6fee867c5efcfe711dac797))
- Add Unity Memory Profiler and related dependencies to project - ([80d663a](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/80d663a8c987278246082a96bce4195360a3e474))
- Update menu item paths for YKYToolkit tools and improve code formatting - ([dab95a7](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/dab95a767c367b99ef821f1a3160725ee2bfb48f))
- Add net.nekobako.EditorPatcher.Editor.xsd schema for custom editor UI element definitions - ([354e0d6](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/354e0d6f07f1957c4026fc5b4fdee1a216bc000a))
- Add new XSD schemas for defining custom UI element types in SDK editor - ([7ee2926](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/7ee2926fdf00d262a1dc6e2a859f383b8e8f44d1))
- Add UnityEngine.UIElements.xsd schema for defining UIElements structure and attributes - ([bd26a96](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/bd26a968a0c3fd4d3bc08f04ecea1a76948698f9))

### ⚙️ Miscellaneous Tasks

- Bump version to 0.4.0 - ([4d4182c](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/4d4182c84b14e511dc09049c56cbce4a12c4f65b))
- Update UnitypackageImporter asset script binding and add EditorClassIdentifier - ([e460c4f](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/e460c4f732b74ab4d8c136e279d5db665fb641c8))
- Fix UnitypackageImporter asset script reference and update csc.rsp configuration - ([8f24071](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/8f24071509aab789b5aed8a4a52084cf9f19dfa2))


## [0.3.1](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.3.0..0.3.1) - 2026-01-02

### ⚙️ Miscellaneous Tasks

- Bump version to 0.3.1 and update utils dependency to >=0.26.8 - ([2ab0203](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/2ab0203e2ca9533c6734c7bc92facad99a445f04))


## [0.3.0](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.2.5..0.3.0) - 2026-01-02

### ⛰️  Features

- Refactor menu item titles and add priority constants for enhanced editor utility organization - ([14b7be6](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/14b7be6033ccc3430b6110cae0f099d34b9e2d21))
- Add "Copy All Components" and "Paste All Components" editor utilities for improved component management - ([cc71b68](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/cc71b6811c0151af99df4f793bf11b7e8c86fca5))
- Add new dependencies for VRChat base, editor tools, and Unity collections - ([d73eca5](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/d73eca5e24349fcbf2c045cb59148fce6440f983))
- Add UnitypackageImporter and XRPackageSettings for improved asset and XR settings configuration - ([cef8db7](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/cef8db7006958b21ec38af8116b754018465d88a))
- Add default SceneTemplateSettings.json for improved scene template configuration - ([a9b21c8](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/a9b21c8cfe93fc7526cc980449d4e5d410281a5d))
- Revamp quality settings for VRChat with new profiles and improved platform-specific optimizations - ([eb3c214](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/eb3c21435102ef6eada1de60fe5ff2c606afbe8e))
- Add default settings file for VRChat package configuration - ([0ca1ea5](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/0ca1ea54e71b2968fd8ff921ced265da3abc4f67))
- Update project settings for XR support, linear color space, dynamic batching, and enhanced audio configuration - ([871fe05](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/871fe05f1c682e028d90db7ae670a763e0fba6f0))
- Add new "Test" scene with basic lighting, camera, and sample objects - ([3dcd85e](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/3dcd85e54de716912fdefb9284590cb586fb78b8))

### ⚙️ Miscellaneous Tasks

- Bump version to 0.3.0 - ([b83ade8](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/b83ade876deac3a30248c93f43d4dd20912d428f))


## [0.2.5](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.2.4..0.2.5) - 2025-12-26

### 🚜 Refactor

- Improve progress bar by trimming paths in EmptyFolderClear and reintroduce utils dependency - ([c922181](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/c922181bb3187d9620da486a216b056c6166d486))
- Reintroduce utils dependency in ClearShaderCache and improve shader cache path handling - ([f1b7cbb](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/f1b7cbbcd44157ace9e4fe27fbb7ad1219cea8dd))

### ⚙️ Miscellaneous Tasks

- Bump version to 0.2.5 - ([7c09746](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/7c09746e7303a1c9304de3666150a1c47871957b))


## [0.2.4](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.2.3..0.2.4) - 2025-12-26

### 🚜 Refactor

- Extract GetEmptyFolders logic for reuse and improve clearing process with dynamic folder detection - ([3612abd](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/3612abdb6e9ce73e890d94cced2f1d961c17ea5c))

### ⚙️ Miscellaneous Tasks

- Bump version to 0.2.4 - ([82a771a](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/82a771abb2629febe8b40266afe32a7974e027a1))


## [0.2.3](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.2.2..0.2.3) - 2025-12-26

### 🚜 Refactor

- Update ClearShaderCache with confirmation dialogs and remove utils dependency - ([a248e08](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/a248e08a3ff07b2a0485feccf9167cb97141ed88))
- Simplify MenuItem attribute in AssetGUIDCopy script - ([b0f3073](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/b0f30733b47a6c5d9858b4bc0daeabe4ad1e156f))

### ⚙️ Miscellaneous Tasks

- Bump version to 0.2.3 - ([90ab1ad](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/90ab1ad8a4d0652654cd89c2c8cc07372c86d39e))


## [0.2.2](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.2.1..0.2.2) - 2025-12-26

### 🐛 Bug Fixes

- Improve progress bar message format in EmptyFolderClear - ([b5b4d45](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/b5b4d45c51e7f43591b25fc398b3aca8daf3033d))

### ⚙️ Miscellaneous Tasks

- Bump version to 0.2.2 - ([175f819](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/175f81948dca13c5d3e5bc0dae7e693c60037cf8))


## [0.2.1](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.2.0..0.2.1) - 2025-12-26

### 🐛 Bug Fixes

- Prevent EmptyFolderClear from running with no empty folders detected - ([99edfa6](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/99edfa60979af171b928cd15744e216a4cb33a4f))

### ⚙️ Miscellaneous Tasks

- Bump version to 0.2.1 - ([ba4635c](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/ba4635cd5a6f657c5a8aeacb4a605b7907f0dd77))


## [0.2.0](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.1.0..0.2.0) - 2025-12-26

### ⛰️  Features

- Add EmptyFolderClear utility to YKY Toolkit - ([d54f26c](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/d54f26c5584aaa71495b00713a4e46d97fa2fa32))

### 🚜 Refactor

- Simplify MenuPath declarations in YKY Toolkit editor scripts - ([6fec858](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/6fec858714eb64edea0739349b3607a94c58bf87))

### ⚙️ Miscellaneous Tasks

- Bump version to 0.2.0 - ([4cef1c3](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/4cef1c3d044ccfb55d744aa742a2ec27de024aad))
- Normalize JSON formatting in Editor assembly definition file - ([0ba5284](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/0ba5284cb797fd86472c8e72c8d167cb0f563336))
- Update io.github.ykysnk.utils to version 0.26.2 in vpm-manifest.json - ([f05d88f](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/f05d88fbe238faeaa5d61e436244b866813057ff))


## [0.1.0](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/compare/0.0.1..0.1.0) - 2025-12-16

### ⛰️  Features

- Add Asset GUID Copy utility to YKY Toolkit - ([68484c7](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/68484c780023ae33dbff65408c13d4fae3ed82d3))

### ⚙️ Miscellaneous Tasks

- Bump version to 0.1.0 - ([ba21a87](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/ba21a876301a9021bb44433f2d2892b77982cd52))


## [0.0.1] - 2025-12-11

### 🐛 Bug Fixes

- Upload files - ([a6585e3](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/a6585e3a6d4df84c1570df7fe31a7ad748594075))
- Upload files - ([3a08c7a](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/3a08c7a4da1b4c94da131daec96bf36aa79281ae))
- Remove demo template package and replace with YKY Toolkit structure. Update license, workflows, and .gitignore accordingly. - ([b84a364](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/b84a3645e9caea3b583ab9842573755545dc3545))

### 📚 Documentation

- Replace README content with YKY Toolkit overview - ([4e1b6c6](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/commit/4e1b6c65d88b3133fd809d02c5a6af288a705e4f))

## New Contributors ❤️

* @T2PeNBiX99wcoxKv3A4g made their first contribution

<!-- generated by git-cliff -->
