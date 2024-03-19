# BGP_Zenv2.0
BGP Modeling with Zen

Currently implemented in multi-entry prefix list mode.
If you want single prefix list entry,

In Match.cs,

1. Line 13, Change PrefixList to PrefixListEntry
2. Line 15, Change PrefixList to PrefixListEntry
3. Line 27, Change Option\<PrefixList\> to Option\<PrefixListEntry\> (two cases)
4. Line 28, Change IsValidPrefixList to IsValidPrefixListEntry
5. Line 72, Change MatchPrefixList to MatchAgainstPrefix
