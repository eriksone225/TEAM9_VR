SPORTS VR Script Pack
=====================

COPY LOCATION
1. In your Unity project, create this folder:
   Assets/SportsVR/Scripts
2. Copy every .cs file in this pack into that folder.
3. Wait for Unity to compile.

REQUIRED UNITY PACKAGES
- XR Interaction Toolkit
- Input System
- TextMeshPro Essentials
- VR Builder

MAIN SCENE OBJECTS
Create these empty GameObjects:
- [SPORTS_VR_MANAGER] with SportsVRGameManager + SimpleHaptics
- [LOBBY_ROOT]
- [BASKETBALL_ROOT]
- [BASEBALL_ROOT]
- [XR_ORIGIN]
- [UI_ROOT]
- [PAUSE_MENU]

BASKETBALL SETUP
- Make 3 basketball balls or 1 prefab copied to 3 spawn points.
- Add Rigidbody + SphereCollider + XR Grab Interactable.
- Add BasketballBallInfo and set points:
  dekat = 1, tengah = 2, jauh = 3.
- Add BallResetter and drag each reset point.
- Add a trigger collider inside the hoop/ring.
- Put BasketballHoopTrigger on the trigger and drag SportsVRGameManager into it.

BASEBALL SETUP
- Baseball prefab: Rigidbody + SphereCollider + BaseballBallHit.
- Bat prefab: Collider + Rigidbody + XR Grab Interactable. Tag it "Bat".
- Pitching machine empty object: add BaseballPitcher.
- Drag baseball prefab, spawnPoint, targetPoint, and SportsVRGameManager.

UI SETUP
- Use World Space canvases.
- Use TextMeshPro text for score, time, instruction.
- Buttons call public methods:
  SportsVRGameManager.StartBasketball()
  SportsVRGameManager.StartBaseball()
  SportsVRGameManager.ShowLobby()
  PauseMenuController.ContinueGame()
  PauseMenuController.RestartSession()
  PauseMenuController.BackToLobby()
  SportsVRGameManager.QuitGame()
  BaseballPitcher.SetSlow(), SetMedium(), SetFast(), StartAutoPitch(), PitchOne()
  LocomotionSettings.UseTeleport(), UseContinuous(), SetVignette(bool)

NOTES
- If you are on Unity 6 and see obsolete warnings for Rigidbody.velocity, ignore them for now.
- If a script cannot find TextMeshProUGUI, open Window > TextMeshPro > Import TMP Essential Resources.
- This pack is a beginner MVP base, not a polished final commercial game.