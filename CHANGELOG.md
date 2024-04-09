# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.3.9] - 2023-04-09

### Fixed
- Error in legacy NDMF

## [0.3.8] - 2023-04-09

### Fixed
- Error in Unity 2019

## [0.3.7] - 2023-04-09

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

## [0.3.6] - 2023-03-09

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

## [0.3.5] - 2023-03-07

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

## [0.3.4] - 2023-03-06

### Added
- Implemented helpbox displaying message about components being ignored during build if inactive
- LI Comment component to leave comments on Prefab and GameObject (supports language switching and markdown)

### Changed
- Excluded components attached to inactive objects during build (excluding AutoDresser and Prop)

## [0.3.3] - 2023-03-02

### Added
- Enabled generation of menus within Menu Group of Modular Avatar

### Changed
- Excluded components attached to EditorOnly objects during build

## [0.3.2] - 2023-03-02

### Added
- Optimization feature via Direct Blend Tree

### Changed
- Speed up processing during build

## [0.3.0] - 2023-02-29

### Added
- AutoDresser component for easy changing of clothes just by attaching the component

### Changed
- Renamed tool from lilAvatarMofifier to lilycalInventory
- Set icons for components
- Enabled toggling objects from multiple components
