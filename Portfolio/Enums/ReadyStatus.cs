#region Imports

using System.ComponentModel;

#endregion

namespace Portfolio.Enums;

public enum ReadyStatus
{
    [Description("Post is incomplete")] Incomplete,

    [Description("Post is production Ready")]
    ProductionReady,

    [Description("Post is in preview mode")]
    PreviewReady
}