<Query Kind="Program">
  <Namespace>LINQPad.Controls</Namespace>
</Query>

void Main()
{
	var filePicker = new FilePicker();
	filePicker.Text = GetDefaultZipFile();
	var projectName = new TextBox().Dump("Project");
	var clientName = new TextBox().Dump("Client");
	var githubAccount = new TextBox().Dump("Github Account");
	filePicker.TextInput += (sender, args) =>
	{
		var zipPath = filePicker.Text.Dump();
		var fi = new FileInfo(zipPath);

		System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, fi.DirectoryName + "/Output");


		DirectoryInfo root = new DirectoryInfo(fi.DirectoryName);

		var directories = root.GetDirectories("*.*", System.IO.SearchOption.AllDirectories);
		foreach (var d in directories)
		{
			Console.WriteLine("D: " + d.FullName);
			var dn = FixNames(d.FullName, projectName.Text, clientName.Text, githubAccount.Text);

			foreach (var f in d.EnumerateFiles())
			{
				Console.WriteLine("F: " + f.FullName);
				var contents = File.ReadAllText(f.FullName);
				contents = FixNames(contents, projectName.Text, clientName.Text, githubAccount.Text);
				File.WriteAllText(f.FullName, contents);
				var fn = FixNames(f.FullName, projectName.Text, clientName.Text, githubAccount.Text);
				if (!fn.Equals(f.FullName))
				{
					var pd = new FileInfo(fn).DirectoryName;
					if (!Directory.Exists(pd))
						Directory.CreateDirectory(pd);

					f.MoveTo(fn);
				}
			}

			if (!dn.Equals(d.FullName) && !Directory.Exists(dn))
			{
				d.MoveTo(dn);
			}
		}

		bool somethingToCheck = true;
		while (somethingToCheck)
		{
			somethingToCheck = false;
			var directoriesAgain = root.GetDirectories("*.*", System.IO.SearchOption.AllDirectories).Reverse();
			foreach (var da in directoriesAgain)
			{
				if (da.GetDirectories().Length == 0 && da.GetFiles().Length == 0)
				{
					somethingToCheck = true;
					Directory.Delete(da.FullName);
				}
			}
		}

	};
	filePicker.Dump("Zip File");
}

// You can define other methods, fields, classes and namespaces here
public string FixNames(string name, string project, string client, string githubAccount)
{
	var o1 = name.Replace("__PROJECTROOT__", project);
	var o2 = o1.Replace("__CLIENTNAME__", client);
	var o3 = o2.Replace("__GITHUBACCOUNT__", githubAccount);
	return o3;
}

public string GetDefaultZipFile() {
	var s = Util.CurrentQueryPath;
	FileInfo fi = new FileInfo(s);
	return $"{fi.Directory.FullName}/PROJECTROOT.zip";
}