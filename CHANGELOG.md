# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
