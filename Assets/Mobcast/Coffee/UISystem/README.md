UISystem
===

Flexible way to manage your stackable UI with transition animation, navigation and history for Unity.

### NOTE: This project is WIP! There's a possibility of change in plan.

Screenshot

[Overview](#overview) | [WebGL Demo](#demo) | [Download](../../releases) | [Usage](#usage) | [Release Notes](#release-notes) | [Issues](../../issues) | [Development Plan](../../projects/1)




<br><br><br><br><br><br>
## Overview

* Supports 3 kind of UI type.
    * Screen
    * Stackable dialog
    * Singleton like as "header", "footer", "toolbar", "blackout"
* Supports template
    * Create abstract class and template for your project.
    * Create UI from the template and prefab.
* Supports multi scene editing.
    * Screen/Dialog
* Manage UI history with argument
    * Go/Back/Suspend/Resume screen
    * Open/Close dialog
    * Show/Hide singleton
* IEnumrator triggers
    * OnInitialize
    * OnFinalize
    * OnShow
    * OnHide
    * OnFocus
* Manage sorting order for stacking UI
    * Track parent sorting order
    * Supports Canvas, or any Renderers(SpriteRenderer, MeshRenderer, ParticleSystemRenderer, etc...).
    * Clipped by Mask2DRect.
    * Fade by CanvasGroup
* EditorOnly component.
    * GameObject will be deactivated or destroyed on play in editor.
    * GameObject has 'EditorOnly' tag. The gameobject will be removed on built.  
    https://answers.unity.com/questions/39106/what-is-editoronly-tag-used-for.html




<br><br><br><br>
## Demo

WebGL Demo




<br><br><br><br>
## Usage

1. Download unitypackage from [Releases](../releases) and install to your project.
1. Import the package into your Unity project. Select `Import Package > Custom Package` from the `Assets` menu.
1. Enjoy!


##### Requirement

* Unity5.5+ *(included Unity 2017.x)*
* No other SDK




<br><br><br><br>
## FAQ

#### Life cycle of UI




#### How to implement transition animation

* Solution 1 : Using UITransition
* Solution 2 : Using AnimationClip and AnimatorController
* Solution 3 : By your script

#### How to improve performance

* Pool your dialog to reuse
    * Override `isPoolable` property.
* Unload resources on hide to save memory.
* Delete unused Object in prefabs/scenes.



<br><br><br><br>
## Release Notes

### ver.0.5.0:

* Changed: Simplification of some complex events.
* Fixed: In Initialize method, StartCoroutine does not work.
* Feature: Template prefab.

### ver.0.4.0:

* Feature: 'Output directory' for UITemplateWizard.
* Feature: 'Camera setting' for UISettings.

### ver.0.3.0:

* Feature: OrderTracker
    * Clipped by Mask2DRect
    * Fade by CanvasGroup

### ver.0.2.0:

* Feature: Template
    * Create abstract class and template for the project.
    * Create UI from the templates.
* Fix: UISettings is not saved.

### ver.0.1.0:

* Feature: Supports 3 kind of UI type.
    * UIScreen
    * UIDialog
    * UISingleton
* Feature: Supports multi scene editing.
* Feature: Manage UI history.
    * Back
    * Suspend
    * Resume
* Feature: IEnumrator triggers
    * OnInstantiate
    * OnInitialize
    * OnFinalize
    * OnShow
    * OnHide
* Feature: Manage sorting order
    * Track parent sorting order
    * Supports Canvas, or any Renderers(SpriteRenderer, MeshRenderer, ParticleSystemRenderer, etc...).
* Feature: EditorOnly component.
    * GameObjects with this component can be deactivated or destroyed on play in Editor.
    * GameObjects with this component have 'EditorOnly' tag. The gameobject will be removed on built.




<br><br><br><br>
## License

* MIT



## Author

[mob-sakai](https://github.com/mob-sakai)



## See Also

* GitHub Page : https://github.com/mob-sakai/UISystem