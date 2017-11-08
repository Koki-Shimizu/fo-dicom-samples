using Dicom.Log;
using Dicom.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dicom.CFindSCP
{
    public class CFindSCP : DicomService, IDicomServiceProvider, IDicomCFindProvider, IDicomCEchoProvider
    {
        private static string StoragePath = @".\DICOM";

        private static DicomTransferSyntax[] AcceptedTransferSyntaxes = new DicomTransferSyntax[]
                                                                            {
                                                                                    DicomTransferSyntax
                                                                                        .ExplicitVRLittleEndian,
                                                                                    DicomTransferSyntax
                                                                                        .ExplicitVRBigEndian,
                                                                                    DicomTransferSyntax
                                                                                        .ImplicitVRLittleEndian
                                                                            };

        private static DicomTransferSyntax[] AcceptedImageTransferSyntaxes = new DicomTransferSyntax[]
                                                                                 {
                                                                                         // Lossless
                                                                                         DicomTransferSyntax
                                                                                             .JPEGLSLossless,
                                                                                         DicomTransferSyntax
                                                                                             .JPEG2000Lossless,
                                                                                         DicomTransferSyntax
                                                                                             .JPEGProcess14SV1,
                                                                                         DicomTransferSyntax
                                                                                             .JPEGProcess14,
                                                                                         DicomTransferSyntax
                                                                                             .RLELossless,

                                                                                         // Lossy
                                                                                         DicomTransferSyntax
                                                                                             .JPEGLSNearLossless,
                                                                                         DicomTransferSyntax
                                                                                             .JPEG2000Lossy,
                                                                                         DicomTransferSyntax
                                                                                             .JPEGProcess1,
                                                                                         DicomTransferSyntax
                                                                                             .JPEGProcess2_4,

                                                                                         // Uncompressed
                                                                                         DicomTransferSyntax
                                                                                             .ExplicitVRLittleEndian,
                                                                                         DicomTransferSyntax
                                                                                             .ExplicitVRBigEndian,
                                                                                         DicomTransferSyntax
                                                                                             .ImplicitVRLittleEndian
                                                                                 };

        public static void Initalize( string storagePath )
        {
            if( string.IsNullOrEmpty(storagePath) )
            {
                throw new ArgumentNullException("storagePath");
            }

            StoragePath = storagePath;
        }
        
        public CFindSCP(INetworkStream stream, Encoding fallbackEncoding, Logger log)
            : base(stream, fallbackEncoding, log)
        {
        }

        public void OnReceiveAssociationRequest(DicomAssociation association)
        {
            if (association.CalledAE != "STORESCP")
            {
                SendAssociationReject(
                    DicomRejectResult.Permanent,
                    DicomRejectSource.ServiceUser,
                    DicomRejectReason.CalledAENotRecognized);
                return;
            }

            foreach (var pc in association.PresentationContexts)
            {
                if (pc.AbstractSyntax == DicomUID.Verification) pc.AcceptTransferSyntaxes(AcceptedTransferSyntaxes);
                else if (pc.AbstractSyntax.StorageCategory != DicomStorageCategory.None) pc.AcceptTransferSyntaxes(AcceptedImageTransferSyntaxes);
            }

            SendAssociationAccept(association);
        }

        public void OnReceiveAssociationReleaseRequest()
        {
            SendAssociationReleaseResponse();
        }

        public void OnReceiveAbort(DicomAbortSource source, DicomAbortReason reason)
        {
        }

        public void OnConnectionClosed(Exception exception)
        {
        }

        private static readonly DicomTag PatientNumberTag = new DicomTag(0x10, 0x20);

        private static readonly DicomTag PatientNameTag = new DicomTag(0x10, 0x10);

        public IEnumerable<DicomCFindResponse> OnCFindRequest(DicomCFindRequest request)
        {
            var patientId = GetPatientId(request);
            var familyName = GetPatientName(request);

            List<DicomCFindResponse> responses = new List<DicomCFindResponse>();
            if (request.Level == DicomQueryRetrieveLevel.Patient)
            {
                foreach (DicomDataset result in GetWorklistResults(request))
                {
                    var response = new DicomCFindResponse(request, DicomStatus.Success);
                    response.Dataset = result;
                    responses.Add(response);
                }
            }
            responses.Add(new DicomCFindResponse(request, DicomStatus.Success));
            return responses;

        }

        private List<DicomDataset> GetWorklistResults(DicomCFindRequest request)
        {
            return MakeDummyWorklist();
        }

        private static List<DicomDataset> MakeDummyWorklist()
        {
            var ds = new DicomDataset();
            ds.Add(DicomTag.SpecificCharacterSet, "ISO_IR 100");
            ds.Add(DicomTag.AccessionNumber, "");
            ds.Add(DicomTag.ReferringPhysicianName, "");

            var sequenceDatase0 = new DicomDataset();
            var sq0 = new DicomSequence(DicomTag.ReferencedStudySequence, sequenceDatase0);
            ds.Add(DicomTag.ReferencedStudySequence, sq0);

            var sequenceDataset1 = new DicomDataset();
            var sq1 = new DicomSequence(DicomTag.ReferencedPatientSequence, sequenceDataset1);
            ds.Add(DicomTag.ReferencedPatientSequence, sq1);

            ds.Add(new DicomPersonName(DicomTag.PatientName, DicomEncoding.GetEncoding("ISO 2022 IR 100"), "name^surname"));
            ds.Add(DicomTag.PatientID, "rtrtrtrtrt");
            ds.Add(DicomTag.PatientBirthName, "errerere");

            ds.Add(DicomTag.PatientSex, "F");
            ds.Add(DicomTag.PatientSize, "170");
            ds.Add(DicomTag.PatientWeight, "170");
            ds.Add(DicomTag.LastMenstrualDate, "");

            ds.Add(DicomTag.StudyInstanceUID, "");
            ds.Add(DicomTag.RequestingPhysician, "");
            ds.Add(DicomTag.RequestedProcedureDescription, "");

            var sequenceDataset2 = new DicomDataset();
            var sq2 = new DicomSequence(DicomTag.RequestedProcedureCodeSequence, sequenceDataset2);

            ds.Add(DicomTag.RequestedProcedureCodeSequence, sq2);
            ds.Add(DicomTag.AdmissionID, "");

            var sequenceDataset3 = new DicomDataset();
            var sq3 = new DicomSequence(DicomTag.ScheduledProcedureStepSequence, sequenceDataset3);
            ds.Add(DicomTag.ScheduledProcedureStepSequence, sq3);

            ds.Add(DicomTag.RequestedProcedureID, "");
            ds.Add(DicomTag.ReasonForTheRequestedProcedure, "");

            var datasets = new List<DicomDataset>();
            datasets.Add(ds);
            return datasets;
        }

        private string GetPatientName(DicomCFindRequest request)
        {
            var familyName = string.Empty;
            if (request.Dataset.Contains(PatientNameTag))
            {
                DicomPersonName dicomPersonNameItem = request.Dataset.Get<DicomPersonName>(PatientNameTag);
                if (!string.IsNullOrEmpty(dicomPersonNameItem.Last) && dicomPersonNameItem.Last != "*")
                {
                    familyName = dicomPersonNameItem.Last;
                }
            }
            return familyName;
        }

        private string GetPatientId(DicomCFindRequest request)
        {
            // TODO: Get patientId from dataset.

            if (request.Dataset.Contains(PatientNumberTag))
            {
                DicomLongString dicomLongStringItem = request.Dataset.Get<DicomLongString>(PatientNumberTag);
               
            }

            return "dummy";
        }

        private string GetTargetDate(DicomCFindRequest request)
        {
            // TODO: Get patientId from dataset.

            if (request.Dataset.Contains(PatientNumberTag))
            {
                DicomLongString dicomLongStringItem = request.Dataset.Get<DicomLongString>(PatientNumberTag);

            }

            return "dummy";
        }

        public void OnCStoreRequestException(string tempFileName, Exception e)
        {
            // let library handle logging and error response
        }

        public DicomCEchoResponse OnCEchoRequest(DicomCEchoRequest request)
        {
            return new DicomCEchoResponse(request, DicomStatus.Success);
        }

        
    }
}
