UISystem
===

## Overview

* Supports 3 kind of UI type.
    * UIScreen
    * UIDialog
    * UISingleton
* Template
    * Create abstract class and template for the project.
    * Create UI from the templates.
* Supports multi scene editing.
* Manage UI history.
    * Back
    * Suspend
    * Resume
* IEnumrator triggers
    * OnInstantiate
    * OnInitialize
    * OnFinalize
    * OnShow
    * OnHide
* Manage sorting order
    * Track parent sorting order
    * Supports Canvas, or any Renderers(SpriteRenderer, MeshRenderer, ParticleSystemRenderer, etc...).
    * Clipped by Mask2DRect.
    * Fade by CanvasGroup
* EditorOnly component.
    * GameObjects with this component can be deactivated or destroyed on play in Editor.
    * GameObjects with this component have 'EditorOnly' tag. The gameobject will be removed on built.


## Requirement

* Unity5.5+
* No other SDK



## Screenshot




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