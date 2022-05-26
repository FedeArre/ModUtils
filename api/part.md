# Part <!-- {docsify-ignore} -->

Represents a car part in the game. 

## Variables <!-- {docsify-ignore} -->

Name | Description | Type
----- | ----------- | ----
Prefab | Stores a reference to the Prefab of the part | GameObject
CarProps | Stores a reference to the CarProperties of the prefab of the part | CarProperties
PartInfo | Stores a reference to the PartInfo of the prefab of the part | PartInfo

## Methods <!-- {docsify-ignore} -->
Name | Description
----- | -----------
[AddTransparent](api/part/setuptransparent.md) | Allows to create a transparent (Attachment point) for the part. Returns a [TransparentData](api/transparentdata.md) instance
[EnablePartPainting](api/part/enablepartpainting.md) | Makes the part paintable
[Localize](api/part/localize.md) | Allows to localize a part for a specific language
[UsePrytoolAttachment](api/part/useprytoolattachment.md) | Sets the part to use the prytool attachment system of the game
[UseHandAttachment](api/part/usehandattachment.md) | Sets the part to use the hand attachment system of the game
[EnableDataSaving](api/part/enabledatasaving.md) | Enables the data saving feature for this part