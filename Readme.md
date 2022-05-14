# Readme

### Scripts I needed

```
kubectl patch ingressroute -n kube-system traefik-dashboard --type=merge -p '{\"spec\":{\"entryPoints\":[\"web\", \"websecure\"]}}'
```