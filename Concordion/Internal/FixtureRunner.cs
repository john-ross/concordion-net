﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concordion.Api;
using Concordion.Internal.Extension;

namespace Concordion.Internal
{
    public class FixtureRunner
    {
        private object m_Fixture;

        private EmbeddedResourceSource m_Source;

        private FileTarget m_Target;

        private SpecificationConfig m_SpecificationConfig;

        public IResultSummary Run(object fixture)
        {
            try
            {
                this.m_Fixture = fixture;
                this.m_Source = new EmbeddedResourceSource(fixture.GetType().Assembly);
                this.m_SpecificationConfig = new SpecificationConfig().Load(fixture.GetType());
                this.m_Target = new FileTarget(m_SpecificationConfig.BaseOutputDirectory);

                var fileExtensions = m_SpecificationConfig.SpecificationFileExtensions;
                if (fileExtensions.Count > 1)
                {
                    return RunAllSpecifications(fileExtensions);
                }
                else if (fileExtensions.Count == 1)
                {
                    return RunSingleSpecification(fileExtensions.First());
                }
                else
                {
                    throw new InvalidOperationException(string.Format("no specification extensions defined for: {0}", this.m_SpecificationConfig));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                var exceptionResult = new SummarizingResultRecorder();
                exceptionResult.Record(Result.Exception);
                return exceptionResult;
            }
        }

        private IResultSummary RunAllSpecifications(IEnumerable<string> fileExtensions)
        {
            var testSummary = new SummarizingResultRecorder();
            var anySpecExecuted = false;
            foreach (var fileExtension in fileExtensions)
            {
                var specLocator = new ClassNameBasedSpecificationLocator(fileExtension);
                var specResource = specLocator.LocateSpecification(m_Fixture);
                if (m_Source.CanFind(specResource))
                {
                    var fixtureResult = RunSingleSpecification(fileExtension);
                    AddToTestResults(fixtureResult, testSummary);
                    anySpecExecuted = true;
                }
            }
            if (!anySpecExecuted)
            {
                testSummary.Record(Result.Exception);
                Console.WriteLine("no active specification found for fixture: {0}", m_Fixture.GetType().FullName);
            }
            return testSummary;
        }

        private IResultSummary RunSingleSpecification(string fileExtension)
        {
            var concordionExtender = new ConcordionBuilder();
            concordionExtender
                .WithSource(m_Source)
                .WithTarget(m_Target)
                .WithSpecificationLocator(new ClassNameBasedSpecificationLocator(fileExtension));
            var extensionLoader = new ExtensionLoader(m_SpecificationConfig);
            extensionLoader.AddExtensions(m_Fixture, concordionExtender);
            var concordion = concordionExtender.Build();
            return concordion.Process(m_Fixture);
        }

        private void AddToTestResults(IResultSummary singleResult, IResultRecorder resultSummary)
        {
            if (resultSummary == null) return;

            if (singleResult.HasExceptions)
            {
                resultSummary.Record(Result.Exception);
            }
            else if (singleResult.HasFailures)
            {
                resultSummary.Record(Result.Failure);
            }
            else
            {
                resultSummary.Record(Result.Success);
            }
        }
    }
}
