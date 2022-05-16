#addin nuget:?package=Cake.FileHelpers
#addin nuget:?package=Cake.Git

using Cake.FileHelpers;
using Cake.Git;

const string defaultDomain = "quillte.ch";
const string defaultBranch = "main";
string currentBranch = Context.GitBranchCurrent(".").FriendlyName;

var target = Argument("target", "Build");
var domain = Argument<string>("domain", defaultDomain);
var branch = Argument<string>("branch", currentBranch);

Task("ReplaceDomainInFiles")
    .Does(() => {
        Context.ReplaceTextInFiles("**/*.yaml", defaultDomain, domain);
        Information(log => log("Updating cluster domain from {0} to {1}", defaultDomain, domain));
    });

Task("ReplaceBranchNameInFiles")
    .Does(() => {
        Context.ReplaceTextInFiles("**/*.yaml", $"revision: {defaultBranch}", branch);
        Information(log => log("Updating branch config from {0} to {1}", defaultBranch, branch));
    });


RunTarget(target);