namespace BikeHackers.Components.Thoth

open System
open BikeHackers.Components

#if FABLE_COMPILER
open Thoth.Json
#else
open Thoth.Json.Net
#endif

module Extras =

  module Decode =

    let set decoder : Decoder<Set<_>> =
      Decode.list decoder
      |> Decode.map Set.ofList

  module Encode =

    let orNone (enc : Encoder<_>) : Encoder<_> =
      function
      | Some x -> enc x
      | None -> Encode.nil

    let stringOrNone = orNone Encode.string

    let intOrNone = orNone Encode.int

    let floatOrNone = orNone Encode.float

module Encode =

  open Extras

  let actuationRatio =
    (fun (x, y) -> Encode.string (sprintf "%i:%i" x y))

  let rearDerailleur : Encoder<RearDerailleur> =
    (fun x ->
      Encode.object
        [
          "manufacturer", Encode.string x.Manufacturer
          "productCode", Encode.string x.ProductCode
          "actuationRatio", actuationRatio x.ActuationRatio
          "speeds", Encode.int x.Speed
          "weight", Encode.int x.Weight
          "largestSprocketMaxTeeth", Encode.int x.LargestSprocketMaxTeeth
          "largestSprocketMinTeeth", Encode.intOrNone x.LargestSprocketMinTeeth
          "smallestSprocketMaxTeeth", Encode.intOrNone x.SmallestSprocketMaxTeeth
          "smallestSprocketMinTeeth", Encode.intOrNone x.SmallestSprocketMinTeeth
          "capacity", Encode.intOrNone x.Capacity
          "isClutched", Encode.bool x.Clutched
        ]
    )

  let chain : Encoder<Chain> =
    (fun x ->
      Encode.object
        [
          "manufacturerCode", Encode.string x.ManufacturerCode
          "manufacturerProductCode", Encode.string x.ManufacturerProductCode
          "speed", Encode.int x.Speed
          "weight", Encode.intOrNone x.Weight
        ])

  let cassette : Encoder<Cassette> =
    (fun x ->
      Encode.object
        [
          "manufacturerCode", Encode.string x.ManufacturerCode
          "manufacturerProductCode", Encode.string x.ManufacturerProductCode
          "interface", Encode.string x.Interface
          "sprockets", Encode.list (x.Sprockets |> List.map Encode.int)
          "sprocketPitch", Encode.float x.SprocketPitch
        ])

  let handedness : Encoder<Handedness> =
    (fun x ->
      match x with
      | Ambi -> Encode.string "ambidextrous"
      | Specific Left -> Encode.string "left"
      | Specific Right -> Encode.string "right")

  let integratedShifter : Encoder<IntegratedShifter> =
    (fun x ->
      Encode.object
        [
          "manufacturerCode", Encode.string x.ManufacturerCode
          "manufacturerProductCode", Encode.stringOrNone x.ManufacturerProductCode
          "speed", Encode.int x.Speed
          "cablePull", Encode.float x.CablePull
          "hand", handedness x.Hand
          "weight", Encode.intOrNone x.Weight
        ])

  let dropHandleBarSize : Encoder<DropHandleBarSize> =
    (fun x ->
      Encode.object
        [
          "manufacturerProductCode", Encode.stringOrNone x.ManufacturerProductCode
          "nominalSize", Encode.string x.NominalSize
          "clampDiameter", Encode.float x.ClampDiameter
          "clampAreaWidth", Encode.floatOrNone x.ClampAreaWidth
          "drop", Encode.floatOrNone x.Drop
          "reach", Encode.floatOrNone x.Reach
          "width", Encode.floatOrNone x.Width
          "dropFlare", Encode.floatOrNone x.DropFlare
          "dropFlareOut", Encode.floatOrNone x.DropFlareOut
          "rise", Encode.floatOrNone x.Rise
          "sweep", Encode.floatOrNone x.Sweep
          "outsideWidth", Encode.floatOrNone x.OutsideWidth
          "weight", Encode.intOrNone x.Weight
        ])

  let dropHandleBar : Encoder<DropHandleBar> =
    (fun x ->
      Encode.object
        [
          "manufacturerCode", Encode.string x.ManufacturerCode
          "name", Encode.string x.Name
          "sizes", Encode.list (x.Sizes |> List.map dropHandleBarSize)
        ])

  let tyreType : Encoder<TyreType> =
    function
    | Clincher -> "clincher"
    | Tubeless -> "tubeless"
    | Tubular -> "tubular"
    >> Encode.string

  let tyreApplication : Encoder<TyreApplication> =
    function
    | Track -> "track"
    | Road -> "road"
    | RoughRoad -> "rough-road"
    | LightGravel -> "light-gravel"
    | RoughGravel -> "rough-gravel"
    | Singletrack -> "singletrack"
    >> Encode.string

  let tyreSize : Encoder<TyreSize> =
    (fun x ->
      Encode.object
        [
          "manufacturerProductCode", Encode.stringOrNone x.ManufacturerProductCode
          "bsd", Encode.int x.BeadSeatDiameter
          "type", tyreType x.Type
          "width", Encode.int x.Width
          "weight", Encode.intOrNone x.Weight
          "treadColor", Encode.string x.TreadColor
          "sidewallColor", Encode.string x.SidewallColor
          "tpi", Encode.intOrNone x.Tpi
        ])

  let tyre : Encoder<Tyre> =
    (fun x ->
      Encode.object
        [
          "id", Encode.guid x.ID
          "manufacturerCode", Encode.string x.ManufacturerCode
          "manufacturerProductCode", Encode.stringOrNone x.ManufacturerProductCode
          "name", Encode.string x.Name
          "sizes", Encode.list (x.Sizes |> List.map tyreSize)
          "application", Encode.orNone Encode.list (x.Application |> Option.map (Seq.toList >> List.map tyreApplication))
        ])

module Decode =

  open Extras

  let private tryParseInt (x : string) =
    match Int32.TryParse x with
    | (true, x) -> Some x
    | _ -> None

  let manufacturer : Decoder<_> =
    Decode.object
      (fun get ->
        {
          Code = get.Required.Field "code" Decode.string
          Name = get.Required.Field "name" Decode.string
          Url = get.Optional.Field "url" Decode.string
        })

  let seatpostSize : Decoder<SeatPostSize> =
    Decode.object
      (fun get ->
        {
          ManufacturerProductCode = get.Optional.Field "manufacturerProduceCode" Decode.string
          Diameter = get.Required.Field "diameter" Decode.float
          Offset = get.Required.Field "offset" Decode.int
          Length = get.Required.Field "length" Decode.int
          Weight = get.Optional.Field "weight" Decode.int
        })

  let seatpost : Decoder<SeatPost> =
    Decode.object
      (fun get ->
        {
          ManufacturerCode = get.Required.Field "manufacturerCode" Decode.string
          Name = get.Required.Field "name" Decode.string
          Sizes = get.Required.Field "sizes" (Decode.list seatpostSize)
        })

  let dropHandleBarSize : Decoder<DropHandleBarSize> =
    Decode.object
      (fun get ->
        {
          ManufacturerProductCode = get.Optional.Field "manufacturerProductCode" Decode.string
          NominalSize = get.Required.Field "nominalSize" Decode.string
          ClampDiameter = get.Required.Field "clampDiameter" Decode.float
          ClampAreaWidth = get.Optional.Field "clampAreaWidth" Decode.float
          Width = get.Optional.Field "width" Decode.float
          Drop = get.Optional.Field "drop" Decode.float
          Reach = get.Optional.Field "reach" Decode.float
          DropFlare = get.Optional.Field "dropFlare" Decode.float
          DropFlareOut = get.Optional.Field "dropFlareOut" Decode.float
          Rise = get.Optional.Field "rise" Decode.float
          Sweep = get.Optional.Field "sweep" Decode.float
          OutsideWidth = get.Optional.Field "outsideWidth" Decode.float
          Weight = get.Optional.Field "weight" Decode.int
        })

  let dropHandleBar : Decoder<DropHandleBar> =
    Decode.object
      (fun get ->
        {
          ManufacturerCode = get.Required.Field "manufacturerCode" Decode.string
          Name = get.Required.Field "name" Decode.string
          Sizes = get.Required.Field "sizes" (Decode.list dropHandleBarSize)
        })

  let actuationRatio =
    Decode.string
    |> Decode.andThen (fun s ->
      let err = sprintf "%s must be of the format `int:int`" s

      match s.Split (":") with
      | [| l; r |] ->
        match tryParseInt l, tryParseInt r with
        | (Some l, Some r) -> Decode.succeed (l, r)
        | _ -> Decode.fail err
      | _ -> Decode.fail err
    )

  let rearDerailleur : Decoder<_> =
    Decode.object
      (fun get ->
        {
          Manufacturer = get.Required.Field "manufacturer" Decode.string
          ProductCode = get.Required.Field "productCode" Decode.string
          ActuationRatio = get.Required.Field "actuationRatio" actuationRatio
          Speed = get.Required.Field "speeds" Decode.int
          Weight = get.Required.Field "weight" Decode.int
          LargestSprocketMaxTeeth = get.Required.Field "largestSprocketMaxTeeth" Decode.int
          LargestSprocketMinTeeth = get.Optional.Field "largestSprocketMinTeeth" Decode.int
          SmallestSprocketMaxTeeth = get.Optional.Field "smallestSprocketMaxTeeth" Decode.int
          SmallestSprocketMinTeeth = get.Optional.Field "smallestSprocketMinTeeth" Decode.int
          Capacity = get.Optional.Field "capacity" Decode.int
          Clutched = get.Required.Field "isClutched" Decode.bool
        }
      )

  let frameMeasurements : Decoder<FrameMeasurements> =
    Decode.object
      (fun get ->
        {
          Stack = get.Optional.Field "stack" Decode.float
          Reach = get.Optional.Field "reach" Decode.float
          TopTubeActual = get.Optional.Field "topTubeActual" Decode.float
          TopTubeEffective = get.Optional.Field "topTubeEffective" Decode.float
          SeatTubeCenterToTop = get.Optional.Field "seatTubeCenterToTop" Decode.float
          HeadTubeLength = get.Optional.Field "headTubeLength" Decode.float
          HeadTubeAngle = get.Optional.Field "headTubeAngle" Decode.float
          SeatTubeAngle = get.Optional.Field "seatTubeAngle" Decode.float
          BottomBracketDrop = get.Optional.Field "bottomBracketDrop" Decode.float
          ForkLength = get.Optional.Field "forkLength" Decode.float
          StandoverHeight = get.Optional.Field "standoverHeight" Decode.float
          ForkRake = get.Optional.Field "forkRake" Decode.float
          ForkAxleToCrown = get.Optional.Field "forkAxleToCrown" Decode.float
          ChainStayLength = get.Optional.Field "chainStayLength" Decode.float
          Wheelbase = get.Optional.Field "bottomBracketDrop" Decode.float
          SeatPostDiameter = get.Optional.Field "seatPostDiameter" Decode.float
        }
      )

  let handedness : Decoder<Handedness> =
    Decode.string
    |> Decode.andThen (
      function
      | "ambidextrous" -> Handedness.Ambi |> Decode.succeed
      | "right" -> Handedness.Specific Right |> Decode.succeed
      | "left" -> Handedness.Specific Left |> Decode.succeed
      | x ->
        sprintf "Expected \"left\", \"right\" or \"ambidextrous\" but got \"%s\"" x
        |> Decode.fail
    )

  let integratedShifter : Decoder<IntegratedShifter> =
    Decode.object
      (fun get ->
        {
          ManufacturerCode = get.Required.Field "manufacturerCode" Decode.string
          ManufacturerProductCode = get.Optional.Field "manufacturerProductCode" Decode.string
          Speed = get.Required.Field "speed" Decode.int
          CablePull = get.Required.Field "cablePull" Decode.float
          Hand = get.Required.Field "hand" handedness
          Weight = get.Optional.Field "weight" Decode.int
        })

  let chain : Decoder<Chain> =
    Decode.object
      (fun get ->
        {
          ManufacturerCode = get.Required.Field "manufacturerCode" Decode.string
          ManufacturerProductCode = get.Required.Field "manufacturerProductCode" Decode.string
          Speed = get.Required.Field "speed" Decode.int
          Weight = get.Optional.Field "weight" Decode.int
        })

  let cassette : Decoder<Cassette> =
    Decode.object
      (fun get ->
        {
          ManufacturerCode = get.Required.Field "manufacturerCode" Decode.string
          ManufacturerProductCode = get.Required.Field "manufacturerProductCode" Decode.string
          Sprockets = get.Required.Field "sprockets" (Decode.list Decode.int)
          SprocketPitch = get.Required.Field "sprocketPitch" Decode.float
          Interface = get.Required.Field "interface" Decode.string
        })

  let private padCartridgeType : Decoder<_> =
    Decode.string
    |> Decode.andThen
      (function
      | "shimano" -> Decode.succeed Shimano
      | "campagnolo" -> Decode.succeed Campagnolo
      | "campagnolo-old" -> Decode.succeed CampagnoloOld
      | x -> Decode.fail (sprintf "Unknown pad cartridge type %s" x))

  let private brakeFixingType : Decoder<_> =
    Decode.string
    |> Decode.andThen
      (function
      | "recessed" -> Decode.succeed RecessedAllenBolt
      | "traditional" -> Decode.succeed TraditionalNut
      | x -> Decode.fail (sprintf "Unknown brake fixing type %s" x))

  let caliperRimBrake : Decoder<CaliperRimBrake> =
    Decode.object
      (fun get ->
        {
          MinimumReach = get.Required.Field "minimumReach" Decode.int
          MaximumReach = get.Required.Field "maximumReach" Decode.int
          PadCartridgeType = get.Required.Field "padCartridgeType" padCartridgeType
          FixingType = get.Required.Field "fixingType" brakeFixingType
        }
      )

  let tyreType : Decoder<TyreType> =
    Decode.string
    |> Decode.andThen (fun x ->
      match x with
      | "clincher" -> Decode.succeed TyreType.Clincher
      | "tubeless" -> Decode.succeed TyreType.Tubeless
      | "tubular" -> Decode.succeed TyreType.Tubular
      | _ -> Decode.fail (sprintf "%s is not a valid tyre type" x)
    )

  let tyreApplication : Decoder<TyreApplication> =
    Decode.string
    |> Decode.andThen (fun x ->
      match x with
      | "track" -> Decode.succeed TyreApplication.Track
      | "road" -> Decode.succeed TyreApplication.Road
      | "rough-road" -> Decode.succeed TyreApplication.RoughRoad
      | "light-gravel" -> Decode.succeed TyreApplication.LightGravel
      | "rough-gravel" -> Decode.succeed TyreApplication.RoughGravel
      | "singletrack" -> Decode.succeed TyreApplication.Singletrack
      | _ -> Decode.fail (sprintf "%s is not a valid tyre application" x)
    )

  let tyreSize : Decoder<TyreSize> =
    Decode.object
      (fun get ->
        {
          BeadSeatDiameter = get.Required.Field "bsd" Decode.int
          ManufacturerProductCode = get.Optional.Field "manufacturerProductCode" Decode.string
          Type = get.Required.Field "type" tyreType
          Weight = get.Optional.Field "weight" Decode.int
          Width = get.Required.Field "width" Decode.int
          TreadColor = get.Required.Field "treadColor" Decode.string
          SidewallColor = get.Required.Field "sidewallColor" Decode.string
          Tpi = get.Optional.Field "tpi" Decode.int
        }
      )

  let tyre : Decoder<Tyre> =
    Decode.object
      (fun get ->
        {
          ID = get.Required.Field "id" Decode.guid
          ManufacturerCode = get.Required.Field "manufacturerCode" Decode.string
          ManufacturerProductCode = get.Optional.Field "manufacturerProductCode" Decode.string
          Name = get.Required.Field "name" Decode.string
          Application = get.Optional.Field "application" (Decode.set tyreApplication)
          Sizes = get.Required.Field "sizes" (Decode.list tyreSize)
        }
      )
