## Hide And Seek
## Functional: 
- The queue for the seeker 
- The actual immortality of the seeker 
- 2 languages: !lang ru and en 
- Blocking the transition from team to team 
- Respawn players at their death every second for n seconds (RespawnTime in config)
- The ability to change the tag, tag color, respawn time, hp from the seeker in the config

### Setting up to 3 seekers example: <br>
"TwoSeekers": 7 means that 2 out of 7 people on the server will search (Observers are not counted) (in config)

## Commands:
<code>css_row</code> - Enter/exit the queue for a seeker 
<code>css_rowlist</code> - Check player in the queue
<code>css_hns_reload</code> - reload config
- `css_row` - Enter/exit the queue for a seeker
- `css_rowlist` - Check players in the queue
- `css_hns_reload` - Reload config `"@css/root"`
- `css_hns` - Enable HnsMode `"@css/root"`
- `css_exithns` - Disable HnsMode `"@css/root"`

support `!lang <ru/en>`

## Requires
<a href="https://github.com/roflmuffin/CounterStrikeSharp">CounterStrikeSharp</a> - Tested on v140
