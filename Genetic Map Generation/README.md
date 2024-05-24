This is an addon for **Godot 4.0**.  A **Godot 3.5** version is available on the [master branch](https://github.com/jon-heard/Godot-MagicaVoxel-Importer-with-Extensions).

A fork of CloneDeath's Godot plugin [MagicaVoxel importer with extensions](https://github.com/CloneDeath/MagicaVoxel-Importer-with-Extensions).

This fork adds a number of features that are waiting to be pulled into CloneDeath's plugin.  If you wish to use these features _now_ then use _this_ plugin instead.

- __Hiding layers in MagicaVoxel removes their voxels in Godot.__ - This allows for toggling optional objects in MagicaVoxel, such as clothing and weapons.
- __MagicaVoxel voxels are rendered in layer order in Godot.__ - MagicaVoxel VOX files are now shown with the latest layers "on top" of earlier layers. This lets the user decide which voxels take precedence and is useful when adding tight features where an extra voxel would be inappropriate, such as for facial expressions or tight clothing.
- __An option has been added to only render the first MagicaVoxel keyframe in Godot.__ - MagicaVoxel allows for creating multiple keyframes of voxels. If each keyframe represents a separate pose then it can look strange when they are all rendered together in Godot. This option fixes that by only showing voxels from the first keyframe. It is on by default.
- __Can now access multiple MagicaVoxel keyframes.__ - MagicaVoxel allows for the creation of keyframes for animations. A new importer has been added for MagicaVoxel VOX files (it's selectable in the "Import" tab). The new importer is called "MagicaVoxel MeshLibrary" and represents a MagicaVoxel VOX file as a MeshLibrary with one mesh for each keyframe. The original importer is still available and is used by default. It is called "MagicaVoxel Mesh" and loads a MagicaVoxel VOX file as a single, static Mesh.
- __A new node-type has been added to easily animate keyframed MagicaVoxel VOX files__ - Once all keyframes are in a MeshLibrary (using the new importer), this new node-type, "FramedMeshInstance", lets you easily animate them. It takes the MeshLibrary and an an integer called "Current Frame". "Current Frame" lets you select which keyframe is display. You can set it in the inspector or with an AnimationPlayer to animate the VOX sprite.

NOTE: If you modify a VOX file, and its associated MeshLibrary doesn't fully update, try reloading the scene (menu Scene->Reload Saved Scene). 
