# Readme

### Scripts I needed

```
kubectl patch ingressroute -n kube-system traefik-dashboard --type=merge -p '{\"spec\":{\"entryPoints\":[\"web\", \"websecure\"]}}'

kubectl edit ingressroute -n kube-system traefik-dashboard

match: Host(`traefik.quillte.ch`) && (PathPrefix(`/dashboard`) || PathPrefix(`/api`))
```

This was able to fix my hosts w/ iptables

```
sudo iptables -I INPUT 1 -i cni0 -s 10.42.0.0/16 -j ACCEPT
sudo iptables -I FORWARD 1 -s 10.42.0.0/15 -j ACCEPT
```