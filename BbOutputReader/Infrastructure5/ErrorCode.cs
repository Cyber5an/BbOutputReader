namespace Sre
{

	enum ErrorCode
	{
		NoError = 0,
		FatalGeneralError = 1,
		GeneralNotCatchedExceptionDuringHostBuild = 2,
		GeneralNotCatchedExceptionDuringHostRun = 3,
		GeneralNotCatchedExceptionAndUnableToGetLogger = 4,
		TaskCancelled = 5
	}
}