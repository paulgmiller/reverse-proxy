import kopf
#networking.k8s.io/v1beta1
@kopf.on.create('networking.k8s.io', 'v1beta1', 'ingress')
def create_fn(spec, **kwargs):
    print(f"And here we are! Creating: {spec}")
    return {'message': 'hello world'}  # will be the new status



@kopf.on.update('networking.k8s.io', 'v1beta1', 'ingress')
def update_fn(spec, old, new, diff, **_):
    print(f"How things change! Updating: {spec}")
    return {'message': 'I see you'}  # will be the new status

@kopf.on.delete('networking.k8s.io', 'v1beta1', 'ingress')
def create_fn(spec, **kwargs):
    print(f"Goodbye! Deleting: {spec}")
    return {'message': 'hello world'}  # will be the new status