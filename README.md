# Non-isomorphic Interaction Techniques for Controlling Avatar Facial Expressions in VR

This is a Unity Project containing the source code and prefab for the interaction techniques we present in our paper referenced below.

- If you use one of our techniques for industrial purposes, please star the project and drop us a line by e-mail to tell us in which application you use it.

- If you use one of our techniques for academic purposes, please cite: Marc Baloup, Thomas Pietrzak, Martin Hachet, and Géry Casiez. 2021. Non-isomorphic Interaction Techniques for Controlling Avatar Facial Expressions in VR. In Proceedings of VRST '21. ACM, New York, NY, USA, Article 5, 1–10.

[![DOI](https://img.shields.io/badge/doi-10.1145%2F3489849.3489867-blue)](https://doi.org/10.1145/3489849.3489867)

```
@inproceedings{10.1145/3489849.3489867,
  author = {Baloup, Marc and Pietrzak, Thomas and Hachet, Martin and Casiez, G\'{e}ry},
  title = {Non-Isomorphic Interaction Techniques for Controlling Avatar Facial Expressions in {VR}},
  year = {2021},
  isbn = {9781450390927},
  publisher = {Association for Computing Machinery},
  address = {New York, NY, USA},
  url = {https://doi.org/10.1145/3489849.3489867},
  doi = {10.1145/3489849.3489867},
  booktitle = {Proceedings of the 27th ACM Symposium on Virtual Reality Software and Technology},
  articleno = {5},
  numpages = {10},
  keywords = {VR, Emoji, Emoticons, Emotion, Facial expression, Avatar},
  location = {Osaka, Japan},
  series = {VRST '21}
}
```


## Usage in your Unity project

1. In the assets folder, copy the folders `FacialExpressions`, `RayCursor`, `VRInputsAPI` and `Resources` and paste them in your assets directory.
   - `FacialExpressions` is the core of the projet, containing the assets for the techniques and the implementation of the face we used for our experiments.
   - `RayCursor` contains an altered version of our RayCursor technique. You can find the original project [here](https://github.com/LokiResearch/RayCursor).
   - `VRInputsAPI` contains a home made API to easily interface with the Unity XR API. If you already use another API to get the VR inputs, just ignore this folder, but you’ll have to change the API calls in the code accordingly, and remove the `VRInputsManager` script from the prefab `VR Rig Facial Expression`.
   - `Resources` contains all the emoji used for the technique _RayMoji_.
2. In the root directory of the repository, copy the files `voice_recognition.py` (for _EmoVoice_) and `expression_gestures.1d` (for _EmoGest_) in the root directory of your project.
3. Depending on what you will use in this project, you will need to install some dependencies:
   - For the technique _Emovoice_:
      - [JSON .NET For Unity](https://assetstore.unity.com/packages/tools/input-management/json-net-for-unity-11347)
	  - Python 3.9 installed on your system (you will have to change the value of `PYTHON_PATH` in `Assets/FacialExpressions/Scripts/Techniques/EmoVoice.cs`).
      - [SpeechRecognition python project](https://pypi.org/project/SpeechRecognition/)
   - If you still want to use `VRInputsAPI`, you need to install `XR Legacy Input Helper` from the package manager of Unity.

- Version of Unity: `2019.4.18f1`

## Demo

You can try the control of the expression of the avatar, with the provided scene at `Assets/Scenes/SampleScene`. All the implemented technique are located in the game object hierarchy (see Figure 1). To try one of them, drag the corresponding gameobject in the Inspector of `VR Rig Facial Expression`, component `Technique Activator`, property `Technique To Activate`.
In VR, the technique is activated when pressing the touchpad of the HTC Vive (the input may vary depending on the controller).

![Unity interface of the SampleScene](https://raw.githubusercontent.com/LokiResearch/AvatarFacialExpressions/main/readme_demo.png)  
**Figure 1: Interface of Unity in the scene `SampleScene`. The techniques are the prefabs that are disabled in the hierarchy. To try one of them, drag and drop it into the property `Technique To Activate` at the bottom, then play the demo in VR.**

## License

This project is published under the MIT License (see `LICENSE.md`).

The project also uses external ressources:
- The emojis in `Assets/Resources/Emojis` are from the _Twemoji_ emoji set ([Source](https://github.com/twitter/twemoji), [CC-BY 4.0](https://creativecommons.org/licenses/by/4.0/))
- Original 3D model for the avatar face, by _cgcookie_ ([Source](https://www.blendswap.com/blend/22625), [CC-BY 4.0](https://creativecommons.org/licenses/by/4.0/))
