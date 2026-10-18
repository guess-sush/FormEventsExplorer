using System.ComponentModel.Composition;
using XrmToolBox.Extensibility;
using XrmToolBox.Extensibility.Interfaces;

namespace FormEventExplorerXrmToolBox
{
    [Export(typeof(IXrmToolBoxPlugin))]
    [ExportMetadata("Name", "Form Event Explorer")]
    [ExportMetadata("Description", "Inspect OnLoad, OnSave, field OnChange handlers and business rules for Dataverse main forms.")]
    //[ExportMetadata("SmallImageBase64", null)]
    //[ExportMetadata("BigImageBase64", null)]
    [ExportMetadata("SmallImageBase64", "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAACXBIWXMAAAsTAAALEwEAmpwYAAACJ0lEQVR4nGNgGAWDBfSd/Kk98cR/PmSx/pO/LEEYWQykZsKJn8no+kFi2PQT7YAJJ3+G9p/6cWba4f+CcMtP/fg44eSPL/0nf9nBLO8/9eP4hFM//0849bMDYdHPBpAYNv0kOWAC2OAfx/pPfncHWw6xCGTwR5AYSA4mBhY/+bMBZjkCo+onwwE/qYpHHcBArgNKd3z47zDpPk4cvfQFVnXDxwETBjoKMta9+S9ZdgmOYb5DF0fHVHNA88GvYMtgGBTU2MTRMU0c0HzwKzy+kS3DJk6TKMiAGgyKBuTgxiY+fBJhKZ5sCJLDJT98HDBhoKKgaM/3/76rvv53Xfb1f8Lmb/+XXvn1f/e93xThXXd//dt97/ft3Xd/JuF1QPGe7/+dln5FwV4rv/1ff4NyRyDwn3isDmg79jPacwXEUu3uR/9lqq7+N5v+GswHhQq1HLDr7q/n2279Z8dwQOaOH3Ugy6znvofnaYW662AH+K36RsUQ+P1/5/2fBhgOCFj9PQlkmdmM13AHSFdcATsAlB6o6oC7v30xHOC07LMeyDK7hZ/AFoMcoNZyB+yAsPXUDYG9t39qY00Hjku/HAJZaD7j9X+trof/7RZ8Ajug/dhP6qWBe7+P/v//nxGrA5wgofAeOReAsuKOu1Ty/d1fb3fe+anDgA+4Lf8m67jka2v0hm9nag98P7v99q81u+/9WkURvvtr5e57v1v23/ovg9fyUcAwAAAAEHtocqnqXLwAAAAASUVORK5CYII=")]
    [ExportMetadata("BigImageBase64", "iVBORw0KGgoAAAANSUhEUgAAAFAAAABQCAYAAACOEfKtAAAACXBIWXMAAAsTAAALEwEAmpwYAAAEqElEQVR4nO3Zb2zUZBwH8MobE2JMfGMUVAjvFl2MMULAALsCIyELvNobIdHoQob6zmyA/LkzhOQWhRai/DEjIl7HOONEnPtzN0SQMNoQQzbd5rLeEWDGgH9YuPb6x+RnOhy5tL279ult9dl+3+T7an3unvuk7dOnYxgMBoPBYDAYDKYgB0X9+UNX4XE/YzjRWG7VzxjrO/ir+lt+52eNIZkfM1PhRb2ek7RrR36EJzzjSdoEL2o5TjRWeRljAXCS1s9LOvCSHvc6N07UY9YYkvkxMwnodZLcFN4DCPCCaMMDr4hTeFP1Oz9mpgHLTZKz43lALIJXFtGORzI/JgzAYpPkiuGVQCyDVxSxGJ7f+TFhAdonWRZPciJ6xHMglsPzM79QAR9Uu8KJ+fWe8KSHP2zCGmON9Tpmcpyox7zieZ3f/wCQ7jIIqCMgj4D0lkFAHQF5BKS3DALqswuwqece1BzOVqybhd+JvoNawMaOP+Dp5oGK1Q3Dy3cgYPMcBWzCSzgYIE9ZEVBCQEBAgkWkhnBx8FoEbEZAQEACwH0XlUmIcrUeRUjHeim1gDxlRUBplgHuK3EZWn+zH29dyl4uSb9jqQVsLLEQuP0wazHxsij4HYuAzQgICEgAyFNWBJQQEBCwwi9Umwp2KaSfRy1gYwX2s4ULBunnIaA0RwGb8BIOBshTVgSUEBDmNODeSxq82ZmHjUkV1p1WoO6MCpvPqhC9pMFXIwakMyZVTcmGmpbNoVTGbO0dM16dNsCWfg22nFOBFZSirW1XYP9lDdJy+DDkNZK9Y/BkRQHj/Tps+rI0HlvQd3vy0Bs6RIDKRrY7m19cEcADoj889r/uuKCFDxEI0Rw6Nw7zAwM2dPrHY622KXDiOn33xMKmZPNwIMD4FR3WtjlxVp+6D1XxG7BwxyA8t3sYXjr0G7AJ53FbvlHpBswY5vkMLCIG3NqVd6BEEjlYEht1bKWqD952PRM/H6D7LEzL5ofEgBtd7n0WlNtedMH2QVhx4m/H8XsvUn8vHCYC3PODsdXt7Hvm/V+KbuirWm44AF//lu7L2OqFLDzlG/CdHnWXHWNF618l34gs2jPiALRW8LABgrY3YyzzDfhGp7rbjrH06J2SgAu2Dzofrk8roQMErmzW+QasEXLr7RjLjt0tCbhw588OwA1n6D8D+2RznW/AiKC8bMdY+dm9koBLYqMOwPoO+gHTsl7tG3B1Eh5jhZxhB1kcHSkK+CI/7gDc1p0PHyBQDY14R8IKuct2kFeOuN8Hn901NPmAbT++pV+nHfA8Q5oaQXnb7eG4+sAtx71v+ad/ui4gX/9K94N0StYbiAHrjsN8NqHccUNceuwuVMWz8MJHN2HlyQnXXch7fZRfvrIx3jUKjzJBEkncf43kZUJdUoWztJ99mX/qA+E9RBSUVj94a9sUOP4T3fe+VMb8hKlUolGY5xWxtl2Bo5TjpTPmxwDwCFPpsIJaH0nkbrrBrREUaPguD8nh0H88eWUjm86Ym5jpzOTZ+EWuNpJQPtjQnuvY1q2M7fw+f/3UgN5l/T+BtqZk42RaNmN9WTMSBZg3rXgYDAaDwWAwGGa25F/Wao/UDkrPpwAAAABJRU5ErkJggg==")]
    [ExportMetadata("BackgroundColor", "#742774")]
    [ExportMetadata("PrimaryFontColor", "White")]
    [ExportMetadata("SecondaryFontColor", "White")]
    public class FormEventExplorerPlugin : PluginBase
    {
        public override IXrmToolBoxPluginControl GetControl() => new FormEventExplorerControl();
    }
}
