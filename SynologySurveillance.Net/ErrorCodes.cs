using System.ComponentModel;

namespace SynologySurveillance.Net;

public enum ErrorCodes
{
    [Description("No permission")]
    NoPermission = 105,
    [Description("Internal error")]
    InternalError = 117,
    [Description("Sid not found")]
    SidNotFound = 119, 	
    [Description("Execute failed")]
    ExecuteFailed = 400, 	
    [Description("Param invalid")]
    ParamInvalid = 401, 	
    [Description("Insufficient license")]
    InsufficientLicense = 403, 	
    [Description("Related server conn failed")]
    RelatedServerConnFailed = 406, 	
    [Description("CMS closed reload")]
    CMSClosedReload = 407, 	
    [Description("Msg connect host failed")]
    MsgConnectHostFailed = 416, 	
    [Description("Object not exist")]
    ObjectNotExist = 418, 	
    [Description("Upgrade dp")]
    UpgradeDp = 424, 	
    [Description("License conn server failed")]
    LicenseConnServerFailed = 440, 	
    [Description("License activation failed")]
    LicenseActivationFailed = 441, 	
    [Description("License activation ssl verify failed")]
    LicenseActivationSslVerifyFailed = 448, 	
    [Description("Not apply on migrating cam")]
    NotApplyOnMigratingCam = 450, 	
    [Description("Camcap error default")]
    CamcapErrorDefault = 457 	
}
