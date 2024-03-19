

def extract_unique_tags(sw):
    with open(f'../../{sw}/results/buckets.txt', 'r') as f:
        lines = f.readlines()
    tags = [line.split('-')[0] for line in lines]
    unique_tags = list(set(tags))
    return unique_tags

def count_all_buckets():
    sws = ['frr', 'quagga', 'gobgp', 'batfish']
    all_tags = []
    for sw in sws:
        unique_tags = extract_unique_tags(sw)
        all_tags += unique_tags
    all_tags = list(set(all_tags))
    print(f"Total number of unique tags: {len(all_tags)}")

count_all_buckets()

