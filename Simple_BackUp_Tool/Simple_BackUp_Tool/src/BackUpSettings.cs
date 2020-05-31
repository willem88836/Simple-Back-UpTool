
public class BackUpSettings
{
	public string TargetDirectory = "";
	public string[] OriginDirectories = new string[0];
	public ActionState DefaultOverwriteState = ActionState.Unset;
	public ActionState DefaultSkipState = ActionState.Unset;
}
