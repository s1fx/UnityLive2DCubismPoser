A small script I made when I was working on the Love, Money, Rock 'n' Roll VN.

With this script you can easily create animation keyframes for a Live2D model inside Unity Animation window and play animation in edit mode.

# Dependencies
[Live2D Cubism SDK For Unity](https://www.live2d.com/en/download/cubism-sdk/)

# Installation
Download the latest .unitypackage file from [Releases Page](https://github.com/s1fx/UnityCubismPoser/releases) and import it into your project (Assets -> Import Package -> Custom Package).

# Usage
1. Add **CubismPoser** component to your Live2D Model Prefab.
2. Open the Animation Window (Window -> Animation -> Animation or press Ctrl + 6)
3. Click the record button and change the Live2D model parameter values  — the timeline will track the changes and create keyframes.

https://github.com/s1fx/UnityLive2DCubismPoser/assets/141889958/f577d690-b2f3-4858-a5fa-daa4a25353bf

You can also create keyframes for specific properties by selecting them in the CubismPoser Inspector and clicking the **Create Keyframes For Selected** button (active only in record mode):

https://github.com/s1fx/UnityLive2DCubismPoser/assets/141889958/802c162d-7292-4b01-96d2-4346c62344e5

## Parameters Groups
You can create parameter groups for quick selection:

https://github.com/s1fx/UnityLive2DCubismPoser/assets/141889958/cb126cce-13d2-4994-bcf9-b5c553f387db

Groups settings are stored in the **CubismPoser/Prefabs/PoserGroups.prefab**. Any parameter starting with a name in the list (regardless of case) will be selected when the group is selected:

![260202497-c091887b-e215-4725-8d0e-f8fff61aac10](https://github.com/s1fx/UnityLive2DCubismPoser/assets/141889958/4c405713-57ef-467e-87f2-7c37bb23c0c4)
