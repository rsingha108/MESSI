def write_rmap(rmap):
	with open('../networks/route-analysis/configs/border4.cfg','w') as g:
		
		PD = {"False":"deny","True":"permit"}
		
		g.write('!\n')
		g.write('hostname border4\n')
		if rmap["Community"] != "None":
			crgx = rmap["Community-regex"].replace(" ","_")
			g.write(f'ip community-list 100 {PD[rmap["Community-permit"]]} {crgx}\n')

		if rmap["DeleteCommunity"] != "None":
			g.write(f'ip community-list 101 permit {rmap["DeleteCommunity"]}\n')

		if rmap["Prefix"] != "None":
			g.write(f'ip prefix-list PFXL {PD[rmap["PrefixPD0"]]} {rmap["Prefix0"]}')
			if rmap["LE0"] != "None":
				g.write(f' le {rmap["LE0"]}')
			if rmap["GE0"] != "None":
				g.write(f' ge {rmap["GE0"]}')
			# g.write(' le 32\n')
			plen = rmap["Prefix0"].split('/')[1]
			g.write(f" le {plen} ge {plen}\n")

			g.write(f'ip prefix-list PFXL {PD[rmap["PrefixPD1"]]} {rmap["Prefix1"]}')
			if rmap["LE1"] != "None":
				g.write(f' le {rmap["LE1"]}')
			if rmap["GE1"] != "None":
				g.write(f' ge {rmap["GE1"]}')
			# g.write(' le 32\n')
			plen = rmap["Prefix0"].split('/')[1]
			g.write(f" le {plen} ge {plen}\n")

			g.write(f'ip prefix-list PFXL {PD[rmap["PrefixPD2"]]} {rmap["Prefix2"]}')
			if rmap["LE2"] != "None":
				g.write(f' le {rmap["LE2"]}')
			if rmap["GE2"] != "None":
				g.write(f' ge {rmap["GE2"]}')
			# g.write(' le 32\n')
			plen = rmap["Prefix0"].split('/')[1]
			g.write(f" le {plen} ge {plen}\n")
			
		if rmap["ASPath"] != "None":
			g.write(f'ip as-path access-list 99 {PD[rmap["ASPath-permit"]]} {rmap["ASPath-regex"]}\n')

		
		## all match statements first (AND of them) then set statements 
		g.write(f'route-map Rmap {PD[rmap["RmapPD"]]} 10\n')
		if rmap["Prefix"] != "None":
			g.write(f'	match ip address prefix-list PFXL\n')
		if rmap["Community"] != "None":
			g.write(f'	match community 100\n')
		if rmap["ASPath"] != "None":
			g.write(f'	match as-path 99\n')
		if rmap["SetLP"] != "None":
			g.write(f'	set local-preference {rmap["SetLP"]}\n')
		if rmap["SetMED"] != "None":
			g.write(f'	set metric {rmap["SetMED"]}\n')
		if rmap["SetCommunity"] != "None":
			g.write(f'	set community {rmap["SetCommunity"]} additive\n')
		if rmap["DeleteCommunity"] != "None":
			g.write(f'	set comm-list 101 delete\n')
		if rmap["ASPathPrepend"] != "None":
			g.write(f'	set as-path prepend {rmap["ASPathPrepend"]}\n')
		if rmap["NextHopIP"] != "None" and rmap["NextHopPeer"] == "False" and rmap["NextHopUnchanged"] == "False":
			g.write(f'	set ip next-hop {rmap["NextHopIP"]}\n')
		g.write('end\n')

		
		# g.write(f's2{cs} cat /var/log/{sw}/bgpd.log >> /mnt/Symb-Route-maps/log.txt \n')

