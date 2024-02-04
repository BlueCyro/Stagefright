# What is a stage?
A 'stage' in the context of Resonite is simply a slot with a particularly-constructed name that contains references to DMX channels, and devices that respond to DMX input.

# How to set up a DMX stage
- Create a slot and name it using the following format: `DMXUniverse:(UniverseID):(NumberOfChannels)`
    - E.g. `DMXUniverse:1:24` listens on Universe 1, with 24 channels available
    - The number of channels can be configured up to 512 depending on your needs
    - Limiting the number of channels is useful for saving network performance

After naming the slot with the correct format, you may wish to test to see if it's recognized by opening the 'Create New' menu on your Developer Tool and navigating to StageFright -> Set up all stages.

Setting up all stages will search the world hierarchy for DMX stages such as the one you've just set up and perform the following operations:
- Attaches a DynamicVariableSpace component with a name of "DMXUniverse" on the root of the stage
- Adds a nonpersistent child slot that contains 1-indexed variables which reference ValueStreams
- Triggers a **synchronous** dynamic impulse with `DMXDevice.refresh` as the tag on the stage's hierarchy


# How to set up DMX devices in a stage
Now that you've got your stage set up, you can populate it with devices that respond to DMX input.

I've provided a device template found in the Stagefright public folder under the "DMX Device Template" directory. This template will help you build functional DMX-responsive objects.

Paste the following link into Resonite to get the public folder: `resrec:///U-Cyro/R-301B1006B0153E24EAB063023EC8DECD52AC09A0429488EE17910D58BCF427AC`

## The template
Once you've got the template, there are several points of interest:
- The template device has it's own variable space called 'DMX_MANAGER'.
- The template device has it's own dynamic variables that control:
    - Address: This variable controls the starting address of the device
    - Width: This variable controls how many channels the device consumes starting at the value of 'Address'
- The template device will automatically manage it's internal variables based on the 'Address' and 'Width' variables
- The 'Placeholder Visual' slot contains another called 'Template Functionality'
    - By default, the template has two DMX channel controls: Core brightness and particle brightness
    - You can test these by placing this device under a stage and using your DMX software to control the appropriate channels

This device contains two different methods of accessing the DMX channel values. [One of them](image-2.png) is by way of components, [and another](image-3.png) is a very convenient, **but potentially unsupported** and unorthodox method of accessing the streams directly with protoflux. It's encouraged to experiment and find methods that work for your particular use-cases.

Each DMX channel can be accessed on the device by using a `DynamicReferenceVariable<IValue<float>>`, and referencing them with the name `DMX_MANAGER/#` where # is your channel starting at 1. This will present the stream as a value which you can use to drive the source of `ValueDriver<float>` components, or by the protoflux method presented in the template object. The channels are presented this way because it's more efficient to store static references than multiple hundreds of updating values inside of the space.

# Tips & info
- As-mentioned, DMX channels are presented as floats of range 0 -> 1 for ease-of-use - you shouldn't need to do any complex bitwise math to express almost any DMX attribute you need
- To interface with 'fine' 16 bit DMX channels, you can use [this method](image.png) to combine the coarse and fine channels into a high-precision 0-1 floating point number
