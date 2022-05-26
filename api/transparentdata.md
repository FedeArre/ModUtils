# TransparentData <!-- {docsify-ignore} -->

Represents a transparent (Attaching point). SimplePartLoader will use the data from this object to create attaching points for your part.

## Variables <!-- {docsify-ignore} -->

Name | Description | Type
----- | ----------- | ----
Name | The name that the transparent will have, only read | string
AttachesTo | The name of parent part of the transparent | string
LocalPos | Position on which the transparent will be created (Relative to his parent) | Vector3
LocalRot | Rotation on which the transparent will be created (Relative to his parent) | Quaternion
Scale | Scale of the transparent (Vector3.one by default) | Vector3
TestingEnabled | Enables transparent in-game editor | bool
PartThatNeedsToBeOff | Name of the part that needs to be off to be able to attach the object, empty by default. Corresponds to transparents.PartThatNeedsToBeOffname | string
AttachingObjects | Array containing attaching objects for complex strucutres. Corresponds to transparents.ATTACHABLES | transparents.AttachingObjects[]
DependantsObjects | Array containing dependants objects for complex structures. Corresponds to transparents.DEPENDANTS | transparents.dependantObjects[]
SavePosition | For transparents that share the same name (As spark plugs for example). Corresponds to transparents.SavePosition | int