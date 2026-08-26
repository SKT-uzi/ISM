using Microsoft.AspNetCore.Authorization;
using ISMDemo.Utilities;

namespace ISMDemo.Auth
{
    public class UserPolicyRequirement : IAuthorizationRequirement
    {
        #region Public Properties
        /// <summary>
        /// Gets the allowed user authorizations.
        /// </summary>
        /// <value>
        /// The allowed user authorizations.
        /// </value>
        public List<UserAuthorization> AllowedUserAuthorizations { get; private set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="UserTokenPolicyRequirement"/> class.
        /// </summary>
        public UserPolicyRequirement()
        {
            this.AllowedUserAuthorizations = new List<UserAuthorization>
            {
                //new UserAuthorization { Route = "/", Permission = EnumDefinition.ChutePermission.RunSetup.ToString() },
                //new UserAuthorization { Route = "/", Permission = EnumDefinition.ChutePermission.RunExecution.ToString() },
                //new UserAuthorization { Route = "/Home", Permission = EnumDefinition.ChutePermission.RunSetup.ToString() },
                //new UserAuthorization { Route = "/Home", Permission = EnumDefinition.ChutePermission.RunExecution.ToString() },
                //new UserAuthorization { Route = "/Run/Create", Permission = EnumDefinition.ChutePermission.RunSetup.ToString() },
                //new UserAuthorization { Route = "/Run/Save", Permission = EnumDefinition.ChutePermission.RunSetup.ToString() },
                //new UserAuthorization { Route = "/Run/EditRun", Permission = EnumDefinition.ChutePermission.RunSetup.ToString() },
                //new UserAuthorization { Route = "/Run/EditRun", Permission = EnumDefinition.ChutePermission.RunExecution.ToString() },
                //new UserAuthorization { Route = "/Run/InProcess", Permission = EnumDefinition.ChutePermission.RunExecution.ToString() },
                //new UserAuthorization { Route = "/Run/LoadInProcessRunInfo", Permission = EnumDefinition.ChutePermission.RunExecution.ToString() },
                //new UserAuthorization { Route = "/Run/LockedRunDevice", Permission = EnumDefinition.ChutePermission.RunExecution.ToString() },
                //new UserAuthorization { Route = "/Run/StartRun", Permission = EnumDefinition.ChutePermission.RunExecution.ToString() },
                //new UserAuthorization { Route = "/Run/CancelRun", Permission = EnumDefinition.ChutePermission.RunExecution.ToString() },
                //new UserAuthorization { Route = "/Run/EndRun", Permission = EnumDefinition.ChutePermission.RunExecution.ToString() },
                //new UserAuthorization { Route = "/Run/PauseSystem", Permission = EnumDefinition.ChutePermission.RunExecution.ToString() },
                //new UserAuthorization { Route = "/Run/UnpauseSystem", Permission = EnumDefinition.ChutePermission.RunExecution.ToString() },
                //new UserAuthorization { Route = "/Run/EndPen", Permission = EnumDefinition.ChutePermission.RunExecution.ToString() },
                //new UserAuthorization { Route = "/Run/SkipAnimal", Permission = EnumDefinition.ChutePermission.RunExecution.ToString() }
            };
        }
        #endregion
    }
}
