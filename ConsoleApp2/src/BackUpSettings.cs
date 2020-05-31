
public class BackUpSettings
{
	public string TargetDirectory = "";
	public string[] OriginDirectories = new string[0];
	public bool UseShortNames = false;
	public BackUp.OverwriteState DefaultOverwriteState = BackUp.OverwriteState.Unset;
}
