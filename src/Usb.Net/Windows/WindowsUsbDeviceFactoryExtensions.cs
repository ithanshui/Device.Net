﻿using Device.Net;
using Device.Net.Windows;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Usb.Net.Windows
{

    public static class WindowsUsbDeviceFactoryExtensions
    {

        public static IDeviceFactory CreateWindowsUsbDeviceFactory(
            ILoggerFactory loggerFactory = null,
            GetConnectedDeviceDefinitionsAsync getConnectedDeviceDefinitionsAsync = null,
            GetUsbInterfaceManager getUsbInterfaceManager = null,
            Guid? classGuid = null,
            ushort? readBufferSize = null,
            ushort? writeBufferSize = null
        ) => CreateWindowsUsbDeviceFactory(
            new List<FilterDeviceDefinition>(),
            loggerFactory,
            getConnectedDeviceDefinitionsAsync,
            getUsbInterfaceManager,
            classGuid,
            readBufferSize,
            writeBufferSize);

        public static IDeviceFactory CreateWindowsUsbDeviceFactory(
            this FilterDeviceDefinition filterDeviceDefinition,
            ILoggerFactory loggerFactory = null,
            GetConnectedDeviceDefinitionsAsync getConnectedDeviceDefinitionsAsync = null,
            GetUsbInterfaceManager getUsbInterfaceManager = null,
            Guid? classGuid = null,
            ushort? readBufferSize = null,
            ushort? writeBufferSize = null
        ) => CreateWindowsUsbDeviceFactory(
            new List<FilterDeviceDefinition> { filterDeviceDefinition },
            loggerFactory,
            getConnectedDeviceDefinitionsAsync,
            getUsbInterfaceManager,
            classGuid,
            readBufferSize,
            writeBufferSize);

        public static IDeviceFactory CreateWindowsUsbDeviceFactory(
            this IDeviceFactory deviceFactory,
            ILoggerFactory loggerFactory = null,
            GetConnectedDeviceDefinitionsAsync getConnectedDeviceDefinitionsAsync = null,
            GetUsbInterfaceManager getUsbInterfaceManager = null,
            Guid? classGuid = null,
            ushort? readBufferSize = null,
            ushort? writeBufferSize = null
        ) => new DeviceManager(new ReadOnlyCollection<IDeviceFactory>(new List<IDeviceFactory> { deviceFactory }), loggerFactory)
            .Aggregate(
                CreateWindowsUsbDeviceFactory(loggerFactory, getConnectedDeviceDefinitionsAsync, getUsbInterfaceManager, classGuid, readBufferSize, writeBufferSize)
                );

        public static IDeviceFactory CreateWindowsUsbDeviceFactory(
        this IEnumerable<FilterDeviceDefinition> filterDeviceDefinitions,
        ILoggerFactory loggerFactory = null,
        GetConnectedDeviceDefinitionsAsync getConnectedDeviceDefinitionsAsync = null,
        GetUsbInterfaceManager getUsbInterfaceManager = null,
        Guid? classGuid = null,
        ushort? readBufferSize = null,
        ushort? writeBufferSize = null
    )
        {
            if (filterDeviceDefinitions == null) throw new ArgumentNullException(nameof(filterDeviceDefinitions));

            loggerFactory ??= NullLoggerFactory.Instance;

            if (getConnectedDeviceDefinitionsAsync == null)
            {
                var logger = loggerFactory.CreateLogger<WindowsDeviceEnumerator>();

                var uwpHidDeviceEnumerator = new WindowsDeviceEnumerator(
                    logger,
                    classGuid ?? WindowsDeviceConstants.WinUSBGuid,
                    (d, guid) => DeviceBase.GetDeviceDefinitionFromWindowsDeviceId(d, DeviceType.Usb, logger, guid),
                    async c =>
                    !filterDeviceDefinitions.Any() || filterDeviceDefinitions.FirstOrDefault(f => f.IsDefinitionMatch(c, DeviceType.Usb)) != null);

                getConnectedDeviceDefinitionsAsync = uwpHidDeviceEnumerator.GetConnectedDeviceDefinitionsAsync;
            }

            getUsbInterfaceManager ??= async d =>
                new WindowsUsbInterfaceManager(
                    //TODO: no idea if this is OK...
                    d,
                    loggerFactory,
                    readBufferSize,
                    writeBufferSize);

            return UsbDeviceFactoryExtensions.CreateUsbDeviceFactory(getConnectedDeviceDefinitionsAsync, getUsbInterfaceManager, loggerFactory);
        }
    }
}