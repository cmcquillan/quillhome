#addin nuget:?package=Cake.FileHelpers
#addin nuget:?package=Cake.Git
#addin nuget:?package=Cake.Kubectl&version=1.0.0

using Cake.FileHelpers;
using Cake.Git;

const string defaultDomain = "quillte.ch";
const string defaultRepo = "https://github.com/cmcquillan/quillhome.git";
const string defaultBranch = "main";
string currentBranch = Context.GitBranchCurrent(".").FriendlyName;

var target = Argument("target", "Build");
var domain = Argument<string>("domain", defaultDomain);
var branch = Argument<string>("branch", currentBranch);
var repo = Argument<string>("repo", defaultRepo);
var local = Argument<bool>("local", false);

Task("ReplaceDomainInFiles")
    .Does(() => {
        Context.ReplaceTextInFiles("**/*.yaml", defaultDomain, domain);
        Information(log => log("Updating cluster domain from {0} to {1}", defaultDomain, domain));
    });

Task("ReplaceBranchNameInFiles")
    .Does(() => {
        Context.ReplaceTextInFiles("**/*.yaml", $"revision: {defaultBranch}", $"revision: {branch}");
        Information(log => log("Updating branch config from {0} to {1}", defaultBranch, branch));
    });

Task("ClusterInitialize")
    .IsDependentOn("ReplaceDomainInFiles")
    .Does(() => {
        Context.KubectlApply(new KubectlApplySettings
        {
            Kustomize = "./argocd"
        });

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
            .Append("--repo").Append(repo)
            .Append("--dest-server").Append("https://kubernetes.default.svc")
            .Append("--dest-namespace").Append("argocd")
            .Append("--path").Append("bootstrap");

        if (local) {
            builder
                .Append("-p")
                .Append("excludedStacks.infra.longhorn=true");
                .Append("-p")
                .Append("trow.trow.volumeClaim.storageClassName=''")
                .Append("-p")
                .Append("airbyte.global.storageClass=''")
        }

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