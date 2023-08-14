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

https://github.com/s1fx/UnityLive2DCubismPoser/assets/141889958/8caef26f-5203-4a00-af09-49bfdf39c96d

You can also create keyframes for specific properties by selecting them in the CubismPoser Inspector and clicking the **Create Keyframes For Selected** button (active only in record mode):

https://github.com/s1fx/UnityLive2DCubismPoser/assets/141889958/3382e822-9ea2-4d31-9e82-7664f394ad0d

## Parameters Groups
You can create parameter groups for quick selection:

https://github.com/s1fx/UnityLive2DCubismPoser/assets/141889958/069fd0fc-d860-4632-98f7-8c0dd3bfe5f2

Groups settings are stored in the **CubismPoser/Prefabs/PoserGroups.prefab**. Any parameter starting with a name in the list (regardless of case) will be selected when the group is selected:

![Screenshot 2023-08-12 082356](https://github.com/s1fx/UnityLive2DCubismPoser/assets/141889958/ff60b91b-625d-48d0-ab71-8553ed92f549)
