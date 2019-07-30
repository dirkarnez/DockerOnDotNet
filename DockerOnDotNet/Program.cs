using Docker.DotNet;
using Docker.DotNet.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DockerOnDotNet
{
    class Program
    {
        static void Main(string[] args)
        {
            AsyncCallback().Wait();
            Console.ReadLine();
        }

        static public async Task AsyncCallback()
        {
            using (var conf = new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine")))
            {
                using (var client = conf.CreateClient())
                {
                    var progress = new Progress<JSONMessage>();

                    progress.ProgressChanged += (sender, jsonMessage) => Console.WriteLine("!");

                    await client.Images.CreateImageAsync(
                        new ImagesCreateParameters() { FromImage = "tensorflow/tensorflow", Tag = "latest" },
                        new AuthConfig(),
                        progress);

                    // Create the container
                    var response = await client.Containers.CreateContainerAsync(new CreateContainerParameters(new Config()
                    {
                        Hostname = "localhost"
                    })
                    {
                        Image = "tensorflow/tensorflow:latest",
                        Tty = false,
                        HostConfig = new HostConfig()
                        {
                            PortBindings = new Dictionary<string, IList<PortBinding>>
                            {
                                { "8888/tcp", new List<PortBinding> { new PortBinding { HostIP = "127.0.0.1", HostPort = "8888" } } },
                            }
                        },
                    });

                    var containers = await client.Containers.ListContainersAsync(new ContainersListParameters()
                    {
                        All = true
                    });

                    var container = containers.First(c => c.ID == response.ID);

                    if (container.State != "running")
                    {
                        var started = await client.Containers.StartContainerAsync(container.ID, new ContainerStartParameters());
                        if (!started)
                        {
                            Console.WriteLine("Not Running");
                        }
                        else
                        {
                            Console.WriteLine("Running");
                        }
                    }
                }
            }
        }
    }
}