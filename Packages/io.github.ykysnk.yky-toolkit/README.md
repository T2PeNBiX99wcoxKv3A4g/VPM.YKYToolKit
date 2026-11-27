# YKY Toolkit

Editor toolkit for Unity: a UPM package installer, the import watcher, delete history, a resource monitor, cache
cleanup and an enhanced transform inspector.

[Documentation](https://t2penbix99wcoxkv3a4g.github.io/VPM-Docs/#/yky-toolkit/Overview)

## Installation

The package is `io.github.ykysnk.yky-toolkit`, distributed through the repository at
<https://t2penbix99wcoxkv3a4g.github.io/VPM/>. Add that listing to ALCOM or the VPM CLI, then install the package.

It depends on `io.github.ykysnk.utils` and `io.github.ykysnk.localization`, which VPM installs with it. Unity 2022.3
or newer. `jp.lilxyzw.editortoolbox` is optional: when it is installed, `.unitypackage` files are imported through it.

## What is included

| Window | What it does |
| --- | --- |
| UPM Installer | Install, remove and update UPM packages, and keep reusable package lists |
| Import Watcher | Records what was imported, when and from where, with per-extension colors |
| Delete History | Lists deleted assets for the project, grouped by date and time |
| Unity Resource Monitor | Live memory and CPU counters for the editor process |
| Tools | Cache cleanup, asset backup, GUID copy, constraint conversion and component copy/paste |
| Transform Inspector | The enhanced transform inspector with expressions, alignment and clipboard |

Everything lives under **Tools > YKYToolkit**, with context actions under **GameObject > YKYToolkit**,
**Assets > YKYToolkit** and the component context menu.

## Links to the other packages

| Package | Why |
| --- | --- |
| [`io.github.ykysnk.utils`](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.Utils) | Inspector base class, GUI helpers and the UPM client |
| [`io.github.ykysnk.localization`](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.Localization) | The editor strings; every window is translated |

## Documentation

The documentation site covers every window and tool:

- [Overview](https://t2penbix99wcoxkv3a4g.github.io/VPM-Docs/#/yky-toolkit/Overview) - what the toolkit contains
- [Installation](https://t2penbix99wcoxkv3a4g.github.io/VPM-Docs/#/yky-toolkit/Installation) - install and first steps
- [UPM Installer](https://t2penbix99wcoxkv3a4g.github.io/VPM-Docs/#/yky-toolkit/UPM-Installer) - installing and updating packages
- [Import Watcher](https://t2penbix99wcoxkv3a4g.github.io/VPM-Docs/#/yky-toolkit/Import-Watcher) - the import history and its settings
- [Tools](https://t2penbix99wcoxkv3a4g.github.io/VPM-Docs/#/yky-toolkit/Tools) - cache cleanup, backups, GUID copy and constraint conversion
- [Transform Inspector](https://t2penbix99wcoxkv3a4g.github.io/VPM-Docs/#/yky-toolkit/Transform-Inspector) - expressions, alignment and clipboard

The site is available in English, Japanese, Simplified Chinese and Traditional Chinese; the links above open the
English text and the language picker in the corner switches the rest.

## License

MIT, see [LICENSE](https://github.com/T2PeNBiX99wcoxKv3A4g/VPM.YKYToolKit/blob/main/LICENSE).
