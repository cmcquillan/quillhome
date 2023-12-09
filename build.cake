#addin nuget:?package=Cake.FileHelpers
#addin nuget:?package=Cake.Git
#addin nuget:?package=Cake.Kubectl&version=1.0.0
#addin nuget:?package=Cake.Yaml
#addin nuget:?package=YamlDotNet&version=12.0.0

using Cake.FileHelpers;
using Cake.Git;
using System.IO;

public class QuillConfig 
{
    public string Domain { get; set; }
    public string Branch { get; set; }
    public string Repo { get; set; }
    public string Image { get; set; }
}

var config = Context.DeserializeYamlFromFile<QuillConfig>("cake.yaml");

Console.WriteLine(config.Domain);
Console.WriteLine(config.Branch);
Console.WriteLine(config.Repo);

string currentBranch = Context.GitBranchCurrent(".").FriendlyName;

var target = Argument("target", "Build");
var domain = Argument<string>("domain", config.Domain);
var branch = Argument<string>("branch", config.Branch);
var repo = Argument<string>("repo", config.Repo);
var image = Argument<string>("image", config.Image);

Task("ReplaceDomainInFiles")
    .Does(() => {
        Context.ReplaceTextInFiles("**/*.yaml", config.Domain, domain);
        Information(log => log("Updating cluster domain from {0} to {1}", config.Domain, domain));
    });

Task("ReplaceBranchNameInFiles")
    .Does(() => {
        Context.ReplaceTextInFiles("**/*.yaml", $"revision: {config.Branch}", $"revision: {branch}");
        Information(log => log("Updating branch config from {0} to {1}", config.Domain, branch));
    });

var fileName = System.IO.Path.GetFileName(image);
var path = File("./_temp/" + fileName);

Task("DownloadLinuxIso")
    .WithCriteria(!System.IO.File.Exists(path), "Linux ISO is already present")
    .Does(() => {
        var fileName = System.IO.Path.GetFileName(image);
        var path = File("./_temp/" + fileName);
        Context.DownloadFile(image, path);
        Console.WriteLine("Completed ISO Download: {0}", path);
    });

Task("InstallOrVerifyArgo")
    .Does(() => {
        var repo = new ProcessArgumentBuilder()
            .Append("repo")
            .Append("add")
            .Append("argo")
            .Append("https://argoproj.github.io/argo-helm");

        var repoExit = Context.StartProcess("helm", repo.Render());

	var deps = new ProcessArgumentBuilder()
            .Append("dependency")
            .Append("build")
            .Append("./argocd");

        var depExit = Context.StartProcess("helm", deps.Render());

        var builder = new ProcessArgumentBuilder()
            .Append("install")
            .Append("argocd")
            .Append("./argocd")
            .Append("--namespace")
            .Append("argocd")
            .Append("--create-namespace");

        var helmExit = Context.StartProcess("helm", builder.Render());

        if (helmExit > 0) {
            Console.WriteLine("Helm install failed: {0}", helmExit);
            throw new Exception("Could not install argocd: " + helmExit);
        }
    });


Task("ClusterInitialize")
    .IsDependentOn("ReplaceDomainInFiles")
    //.IsDependentOn("InstallOrVerifyArgo")
    .Does(() => {
        // Context.KubectlApply(new KubectlApplySettings
        // {
        //     Kustomize = "./argocd"
        // });

        var builder = new ProcessArgumentBuilder()
            .Append("config").Append("set-context")
            .Append("--current")
            .Append("--namespace").Append("argocd");

        var kubeExit = Context.StartProcess("kubectl", builder.Render());

        if (kubeExit > 0) {
            Console.WriteLine("Uh Oh!");
        }

        builder = new ProcessArgumentBuilder()
            .Append("login")
            .Append("argo.{0}", domain)
            .Append("--core");

        var argoExit = Context.StartProcess("argocd", builder.Render());

        if (argoExit > 0) {
            Console.WriteLine("Uh Oh!");
        }

        builder = new ProcessArgumentBuilder()
            .Append("app").Append("create").Append("root")
            .Append("--upsert")
            .Append("--repo").Append(repo)
            .Append("--dest-server").Append("https://kubernetes.default.svc")
            .Append("--dest-namespace").Append("argocd")
            .Append("--path").Append("bootstrap");

        argoExit = Context.StartProcess("argocd", builder.Render());

        if (argoExit > 0) {
            Console.WriteLine("Uh Oh!");
        }

        builder = new ProcessArgumentBuilder()
            .Append("app").Append("sync").Append("root");

        argoExit = Context.StartProcess("argocd", builder.Render());

        if (argoExit > 0) {
            Console.WriteLine("Uh Oh!");
        }
    });

RunTarget(target);
