const string defaultDomain = "quillte.ch"

var target = Argument("target", "Build");
var domain = Argument<string>("domain", defaultDomain);

var f = Task("ReplaceDomainInFiles")
    .Does(() => {
        Context.ReplaceTextInFiles("**/*.yaml", defaultDomain, domain);
        Information(log => log("Updating cluster domain from {0} to {1}", defaultDomain, domain))
    });

RunTarget(target);