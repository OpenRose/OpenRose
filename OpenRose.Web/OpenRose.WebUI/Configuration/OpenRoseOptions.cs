namespace OpenRose.WebUI.Configuration;

public class OpenRoseOptions
{
	public UserFilesStorageOptions UserFilesStorage { get; set; } = new();
}

public class UserFilesStorageOptions
{
	public bool Enabled { get; set; } = true;
	public string RootPath { get; set; } = "./user-files";
}