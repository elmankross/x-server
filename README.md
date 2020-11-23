## Description

[TODO]

## Configuration

### Root

|Key                |Schema
|-------            |------
|Serilog            |\<Serilog>
|Storage            |\<Storage>

### Serilog

[TODO]

### Storage

|Key                |Chema              |Comment
|-------            |------             |----
|BaseDirectory      |-                  | Base directory for loaded apps
|Applications       |[\<Application>]   |-


### Application

|Key            |Schema                 |Comment
|---            |------                 |-------
|Name           |-                      |Displayable application unique name
|Bitness        |-                      |[RESERVED]
|Version        |-                      |Application version
|Hash           |-                      |Downloadable artifact's hash has format: [algorythm:value]
|DownloadUrl    |-                      |Direct application's download url
|WebUrl         |-                          |Url to web interface to interact with the soft
|Dependencies   |[string]               |[RESERVED]
|Exec           |\<ApplicationExec>     |-


### ApplicationExec

|Key        |Schema                     |Comment
|---        |------                     |-------
|Binary     |-                          |Executable application's file
|Args       |[string]                   |-
|Hooks      |\<ApplicationExecHooks>    |-
|Env        |\<KeyValuePair>            |-


### KeyValuePair

|Key            |Schema             |Comment
|---            |------             |-------
|\<key:string>  |\<value:string>    |Case sensitive key-value pair


### ApplicationExecHooks

|Key        |Schema                         |Comment
|---        |------                         |-------
|Before     |\<ApplicationExecHookValue>  |Executes ones before starting the main process
|After      |\<ApplicationExecHookValue>   |Executes ones after starting the main process

### ApplicationExecHookValue

|Key        |Schema             |Comment
|---        |------             |-
|Binary     |-                  |Executable application's file
|Args       |[string]           |-