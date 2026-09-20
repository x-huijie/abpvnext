namespace Xhj.Project;

public static class ProjectDomainErrorCodes
{
    /* You can add your business exception error codes here, as constants */

    // 认证/Token
    public const string InvalidUserNameOrPassword = "Project:Auth:001";

    public const string AuthServerNotConfigured = "Project:Auth:002";

    public const string TokenRequestFailed = "Project:Auth:003";

    public const string RefreshTokenFailed = "Project:Auth:004";

    public const string InvalidRefreshToken = "Project:Auth:005";
}
