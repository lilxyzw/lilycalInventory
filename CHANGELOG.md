# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.4.5] - 2024-10-17
### Changed
- Changed to not generate animations for Renderers that do not have materials

## [1.4.4] - 2024-10-14
### Fixed
- Error if parent folder is off

## [1.4.3] - 2024-10-13
### Fixed
- Error when adding some properties

## [1.4.2] - 2024-10-13
### Changed
- When attaching the lilycalInventory component, if the GameObject already has an MA Menu Item, it will be used
- Changed the processing of ReorderableList in the component editor (may have fixed some bugs that were environment-dependent)

### Fixed
- A problem where the GameObject reference in the component was lost when copying and pasting a GameObject with a lilycalInventory component to another avatar

## [1.4.1] - 2024-10-05
### Fixed
- An error occurs when the operation target is EditorOnly and is operated from multiple components
- Compilation error under certain conditions
- The top version in the changelog is skipped
- Active state of object not reflected when dragged to header

## [1.4.0] - 2024-09-29
### Added
- Auto-avoiding duplicate parameter names
- Component `LI AsMAMergeAnimator` that adds lilycalInventory to the avatar as MA MergeAnimator

### Changed
- Consider MaterialModifier when getting material properties

### Fixed
- Child menu not generated if parent folder is overridden with MA MenuItem

## [1.3.1] - 2024-09-22
### Changed
- replace object references when copying components to other avatars #121

### Fixed
- preset does not support int parameter compression #123
- incorrect parameter names when generating a menu for CostumeChanger in Modular Avatar #122

## [1.3.0] - 2024-09-19
### Changed
- end of support for Unity 2019
- optimize processing

### Fixed
- error when use VRCAnimatorPlayAudio #115
- error when submenu is empty #116
- supports materials being cloned by other tools
- error due to circular reference in menu

## [1.2.0] - 2024-09-16
### Changed
- Create folders if menu name contains slash
- Conbine same name submenus
- Mesh deletion BlendShape now properly handles cases where values ​​can only be changed to 0 or 100 when operated from multiple components #104
- Compress int parameters
- Make the component class public

### Fixed
- An error occurs if the costume's parent is managed by MA #113
- Update comment in prefab #114

## [1.1.1] - 2024-09-08
### Changed
- Change callback order

### Fixed
- Change the binding to a subclass #101
- Changelog and version not displayed correctly #102
- Missing information in the tooltip #105

## [1.1.0] - 2024-09-01
### Added
- Reload language files (`Tools/lilycalInventory/Reload Language Files`)
- LI AutoFixMeshSettings #96
- AutoDresser in CostumeChanger #27
- Prop to ItemToggler #27
- LI Preset #56
- Generate MenuFolder in Prop and AutoDresser #75
- Warn EditorOnly and AnimationClip

### Changed
- Enable components on inactive GameObjects #92
- Change menu order of AutoDresser

### Fixed
- Update preview when adding BlendShape #93
- Error when AnimationClip is null
- Cursor is not visible in MaterialPropertyModifier #84
- Error under certain conditions
- Error when attaching a LI MenuFolder to the same object as MA MenuItem
- Menu is not generated when only AnimationClip is set #94

## [1.0.1] - 2024-08-12
### Fixed
- Some animations do not work
- Component help button link

## [1.0.0] - 2024-08-10
### Fixed
- Error when the operation target is EditorOnly

## [0.4.0] - 2024-07-08
### Added
- Icons for each component
- Parameter initial values ​​can now be set (by nekobako)
- New Preview Mode
- Simplified Chinese support (by lonelyicer)
- A prefab of a gimmick that works locally

### Changed
- UI Improvements
- Change preview to off by default

### Fixed
- Menus not sorted hierarchically (by nekobako)
- Warn if there is a circular reference in the menu (by nekobako)
- An issue where necessary settings may be deleted
- Behavior when an object is operated by multiple components (by nekobako)
- UI lag in Foldout
- Fixed an issue where material replacement could not be undone when previewing
- Added handling for when the language file cannot be loaded

## [0.3.9] - 2024-04-09

### Fixed
- Error in legacy NDMF

## [0.3.8] - 2024-04-09

### Fixed
- Error in Unity 2019

## [0.3.7] - 2024-04-09

### Added
- General UI improvements.
- Added feature to check the number of parameters.
- Added interpolation between frames in SmoothChanger preview.
- Added warnings in the editor for AutoDresser.
- Enabled bulk opening and closing of Foldouts by holding Alt key while clicking.
- Implemented version check system.
- Added change history review window.
- Added property saving and local-only setting to AutoDresserSettings.
- Added comments to the code.
- Supported insertion of AnimationClips.

### Changed
- Changed frame values to be represented in percentage.
- Excluded components attached to inactive objects during build (excluding AutoDresser and Prop).
- Enabled optimization via DirectBlendTree by default.

### Fixed
- Display bug in LI Comment.
- Display bug in Foldout.
- Rectified warnings displayed during build.
- Error occurring during preview.
- Improper suggestions for BlendShape.
- Error occurring when all AutoDressers are turned off.

## [0.3.6] - 2024-03-09

### Added
- Enabled bulk addition of multiple objects via drag and drop in certain properties.
- Enabled confirmation and editing of folder contents during MenuFolder editing.

### Changed
- Registered assets in ObjectRegistry of NDMP upon cloning.
- Moved material modification process to occur after TexTransTool.
- Improved areas with excessive Foldout.

### Fixed
- Malfunction of MaterialModifier's exclusion function.
- Error occurring when NDMF version was outdated.

## [0.3.5] - 2024-03-07

### Fixed
- Issue causing some properties of LI Comment to become invisible upon language change
- Malfunction of vector manipulation for materials
- Problem where objects with identical names were treated as the same object
- Issue where ItemToggler properties were not registered in AnimatorController under specific conditions
- Display issue where objects were not shown in error report for parameter duplication
- Error occurring with empty renderer for materials
- Issue where previewing SmoothChanger and CostumeChanger would overwrite settings with values from materials within the mesh when specifying a mesh for the Renderer of material property manipulation

### Changed
- Enabled simultaneous attachment of MenuFolder component with other components
- Restored processing of components for inactive GameObjects

## [0.3.4] - 2024-03-06

### Added
- Implemented helpbox displaying message about components being ignored during build if inactive
- LI Comment component to leave comments on Prefab and GameObject (supports language switching and markdown)

### Changed
- Excluded components attached to inactive objects during build (excluding AutoDresser and Prop)

## [0.3.3] - 2024-03-02

### Added
- Enabled generation of menus within Menu Group of Modular Avatar

### Changed
- Excluded components attached to EditorOnly objects during build

## [0.3.2] - 2024-03-02

### Added
- Optimization feature via Direct Blend Tree

### Changed
- Speed up processing during build

## [0.3.0] - 2024-02-29

### Added
- AutoDresser component for easy changing of clothes just by attaching the component

### Changed
- Renamed tool from lilAvatarMofifier to lilycalInventory
- Set icons for components
- Enabled toggling objects from multiple components
