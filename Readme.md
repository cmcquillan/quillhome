# Readme

### Scripts I needed

```
kubectl patch ingressroute -n kube-system traefik-dashboard --type=merge -p '{\"spec\":{\"entryPoints\":[\"web\", \"websecure\"]}}'

kubectl edit ingressroute -n kube-system traefik-dashboard

match: Host(`traefik.quillte.ch`) && (PathPrefix(`/dashboard`) || PathPrefix(`/api`))
```