# Readme

### Scripts I needed

```
kubectl patch ingressroute -n kube-system traefik-dashboard --type=merge -p '{\"spec\":{\"entryPoints\":[\"web\", \"websecure\"]}}'

kubectl edit ingressroute -n kube-system traefik-dashboard

match: Host(`argo.quillte.ch`) && (HostPathPrefix(`/dashboard`) || PathPrefix(`/api`))
```