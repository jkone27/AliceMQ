#r @"packages/FAKE/tools/FakeLib.dll"
#r @"packages/FSharp.Management/lib/net40/FSharp.Management.dll"
open Fake
open FSharp.Management

type WorkSpace = FileSystem<__SOURCE_DIRECTORY__>
let solutionFile = WorkSpace.``AliceMQ.sln``
let testDir = WorkSpace.Tests.Path

Target "Restore" (fun _ ->
    trace "Restore..." ;
    DotNetCli.Restore (fun p -> 
        { p with NoCache = true })

)

Target "Build-Debug" (fun _ ->
    trace "Build-Debug..." ;
    DotNetCli.Build (fun p -> 
       { p with 
            DotNetCli.BuildParams.Configuration = "Debug" 
        })
)
 
Target "Test" (fun _ ->
    trace "Test..."
    !! (testDir @@ "Test.dll")
    |> Fake.Testing.XUnit2.xUnit2 (fun p -> { p with HtmlOutputPath = Some (testDir @@ "xunit.html") })
)


Target "Build-Release" (fun _ ->
    trace "Build-Release..." ;
    DotNetCli.Build
      (fun p -> 
           { p with 
                Configuration = "Release" })
)

//dependencies pipeline
"Restore" ==> "Build-Debug" ==> "Test" ==> "Build-Release"
 
Run "Build-Release"



