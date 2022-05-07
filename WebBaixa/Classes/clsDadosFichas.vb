Option Explicit On
Option Strict On

Namespace DadosFichas

    Public Class Attributes
        Public Property noData As Boolean
    End Class

    Public Class Ficha
        Public Property qtd_comerc As String
        Public Property nm_pais_aquis As String
        Public Property via_transp As String
        Public Property num_tel2_notif As String
        Public Property id_import As String
        Public Property desc_prodt As String
        Public Property exportador_nome As String
        Public Property val_seg_un_us As String
        Public Property fabricante_endereco As String
        Public Property tp_unid_comerc As String
        Public Property val_cif_un_us As String
        Public Property model As String
        Public Property brand As String
        Public Property cidade_notif As String
        Public Property val_base_calculo_antidump As String
        Public Property importador_endereco As String
        Public Property num_ord_cmpl As String
        Public Property num_subitem As String
        Public Property nome_adquirente As String
        Public Property val_vmle_us_subitem As String
        Public Property size As String
        Public Property cidade_import As String
        Public Property val_devido_antidump As String
        Public Property incoterm As String
        Public Property adquirente_endereco As String
        Public Property cdncm_compl As String
        Public Property desc_cnae_import As String
        Public Property porte_empresa_notif As String
        Public Property dsc_depto As String
        Public Property val_frete_us_subitem As String
        Public Property num_item As String
        Public Property tp_nat_inf As String
        Public Property cod_cnae_notif As String
        Public Property nome_unid_desembaraco As String
        Public Property cod_cep_notif As String
        Public Property desc_cnae_notif As String
        Public Property val_vmld_us_subitem As String 'xx
        Public Property val_fob_un_us As String 'xx
        Public Property desc_unid_estat As String
        Public Property val_peso_liq_subitem As String
        Public Property val_frete_un_us As String
        Public Property nm_pais_origem As String
        Public Property product As String
        Public Property fabricante_nome As String
        Public Property num_tel1_notif As String
        Public Property num_ord As String
        Public Property aliquota_antidump As String
        Public Property anomes As String
        Public Property val_recolher_antidump As String
        Public Property material As String
        Public Property importador_cnpj As String
        Public Property sgl_uf_notif As String
        Public Property porte_empresa_import As String
        Public Property val_seg_us_subitem As String
        Public Property importador_nome As String
        Public Property attributes As Attributes
        Public Property endereco_import As String
        Public Property sgl_uf_import As String
        Public Property ficha As List(Of Ficha)
        Public Property scrollHash As String 'xx
        Public Property scrollTimeout As String 'xx
        Public Property itemsPerPage As Integer
        Public Property totalItems As Integer
        Public Property lastPage As Integer
        Public Property fromItem As String 'xx
        Public Property toItem As String 'xx
    End Class

    Public Class Customization
        Public Property customHexColor As String 'xx
        Public Property customName As String 'xx
        Public Property customLogo As String 'xx
    End Class

    Public Class Customer
        Public Property id As Integer
        Public Property name As String
        Public Property isOnboard As Boolean
        Public Property customerOrigin As String 'xx
        Public Property columnRestrictions As List(Of String) 'xx
        Public Property customization As Customization
    End Class

    Public Class Permission
        Public Property id As Integer
        Public Property name As String
        Public Property description As String
        Public Property slug As String
        Public Property dependency As String 'xx
    End Class

    Public Class User
        Public Property id As Integer
        Public Property email As String
        Public Property name As String
        Public Property avatar As String
        Public Property permissions As List(Of Permission)
        Public Property language As String
    End Class









































    Public Class Info
        Public Property apiAccess As Boolean                               'Public bool apiAccess { Get; Set; }
        Public Property inTrial As Boolean                                 'Public bool inTrial { Get; Set; }
        Public Property useBiConditionFilter As Boolean                    'Public bool useBiConditionFilter { Get; Set; }
        Public Property tagsForFilter As Boolean                           'Public bool tagsForFilter { Get; Set; }
        Public Property contractedDepartment As String 'xx                 'Public Object contractedDepartment { Get; Set; }              
        Public Property taxesFunctionality As Boolean                      'Public bool taxesFunctionality { Get; Set; }
        Public Property excelExport As Boolean                             'Public bool excelExport { Get; Set; }
        Public Property excelLines As Integer                              'Public int excelLines { Get; Set; }
        Public Property excelDownloads As Integer                          'Public int excelDownloads { Get; Set; }
        Public Property monthlySearches As Integer                         'Public int monthlySearches { Get; Set; }
        Public Property searchDaysLimit As Integer                         'Public int searchDaysLimit { Get; Set; }
        Public Property searchDaysRange As Integer                         'Public int searchDaysRange { Get; Set; }
        Public Property searchLinesLimit As Integer                        'Public int searchLinesLimit { Get; Set; }
        Public Property userLimit As Integer                               'Public int userLimit { Get; Set; }
        Public Property filterPossibleGuysLimit As Integer                 'Public int filterPossibleGuysLimit { Get; Set; }
        Public Property trialRemainingDays As Integer                      'Public int trialRemainingDays { Get; Set; }
        Public Property trialStart As String 'xx                           'Public DateTime trialStart { Get; Set; }              
        Public Property trialExpiration As String 'xx                      'Public DateTime trialExpiration { Get; Set; }              
        Public Property contractStart As DateTime?                          'Public Object contractStart { Get; Set; }
        Public Property contractExpiration As DateTime?                     'Public Object contractExpiration { Get; Set; }
        Public Property deadlineMonthNewData As Integer                    'Public int deadlineMonthNewData { Get; Set; }
        Public Property expoProductFunctionality As Boolean                'Public bool expoProductFunctionality { Get; Set; }
        Public Property expoDashboardChartYearToDate As Boolean            'Public bool expoDashboardChartYearToDate { Get; Set; }
        Public Property expoRealExporterFunctionality As Boolean           'Public bool expoRealExporterFunctionality { Get; Set; }
        Public Property expoAverageShipmentFunctionality As Boolean        'Public bool expoAverageShipmentFunctionality { Get; Set; }
        Public Property updatePeriodForData As String                      'Public String updatePeriodForData { Get; Set; }
        Public Property uploadSizeLimitMb As String 'xx                    'Public Object uploadSizeLimitMb { Get; Set; }              
        Public Property trackingMonthlyBl As String 'xx                    'Public Object trackingMonthlyBl { Get; Set; }              
        Public Property trackingForceCertificate As String 'xx             'Public Object trackingForceCertificate { Get; Set; }              
        Public Property trackingApiAccess As String 'xx                    'Public Object trackingApiAccess { Get; Set; }              
        Public Property trackingOnlyInternationalLogistics As String 'xx   'Public Object trackingOnlyInternationalLogistics { Get; Set; }              
        Public Property fobUnitValue As Boolean                            'Public bool fobUnitValue { Get; Set; }
        Public Property hasSiscoriBase As Boolean                          'Public bool hasSiscoriBase { Get; Set; }
        Public Property hasComexStatBase As Boolean                        'Public bool hasComexStatBase { Get; Set; }
        Public Property searchDaysLimitComexstat As Integer                'Public int searchDaysLimitComexstat { Get; Set; }
        Public Property shipments As Boolean                               'Public bool shipments { Get; Set; }
        Public Property hasExTarifario As Boolean                          'Public bool hasExTarifario { Get; Set; }

    End Class

    Public Class Column
        Public Property name As String
        Public Property [alias] As String
        Public Property description As String
        Public Property method As String
        Public Property aliasEn As String
        Public Property dbColumnRepresentation As String
        Public Property filterName As String
    End Class

    Public Class Usage
        Public Property quantitySearches As Integer
        Public Property quantityExcel As Integer
        Public Property quantityFilterPossibleGuys As Integer
    End Class

    Public Class Plan
        Public Property planSlug As String
        Public Property info As Info
        Public Property columns As List(Of Column)
        Public Property usage As Usage
    End Class

    Public Class Product
        Public Property id As Integer
        Public Property name As String
        Public Property slug As String
    End Class

    Public Class ProductsStatu
        Public Property status As String
        Public Property trialRemainingDays As Integer
        Public Property show As Boolean
        Public Property order As Integer
        Public Property image As String
        Public Property nameJson As String
        Public Property descriptionJson As String 'xx
        Public Property isNew As Boolean
        Public Property canTrial As Boolean
        Public Property canFreemium As Boolean
        Public Property canContract As Boolean
        Public Property hasCertificate As Boolean
        Public Property id As Integer
        Public Property name As String
        Public Property slug As String
    End Class

    Public Class FilterNcm
        Public Property allowedValues As List(Of String)
        Public Property blocked As Boolean
    End Class

    Public Class AllowedValues
        Public Property min As String
        Public Property max As String
    End Class

    Public Class FilterPeriod
        Public Property allowedValues As AllowedValues
        Public Property blocked As Boolean
    End Class

    Public Class FilterIncoterm
        Public Property allowedValues As List(Of String)
        Public Property blocked As Boolean
    End Class

    Public Class FilterOriginCountry
        Public Property allowedValues As List(Of String)
        Public Property blocked As Boolean
    End Class

    Public Class FilterPossibleImporter
        Public Property allowedValues As List(Of String)
        Public Property blocked As Boolean
    End Class

    Public Class FilterPossibleExporter
        Public Property allowedValues As List(Of String)
        Public Property blocked As Boolean
    End Class

    Public Class FilterAcquisitionCountry
        Public Property allowedValues As List(Of String)
        Public Property blocked As Boolean
    End Class

    Public Class FilterProductDescription
        Public Property allowedValues As List(Of String)
        Public Property blocked As Boolean
    End Class

    Public Class FilterClearanceUnitName
        Public Property allowedValues As List(Of String)
        Public Property blocked As Boolean
    End Class

    Public Class FilterStaticUnit
        Public Property allowedValues As List(Of String)
        Public Property blocked As Boolean
    End Class

    Public Class FilterTransportRouteType
        Public Property allowedValues As List(Of String)
        Public Property blocked As Boolean
    End Class

    Public Class FilterPossibleNotified
        Public Property allowedValues As List(Of String)
        Public Property blocked As Boolean
    End Class

    Public Class FilterPossibleManufactor
        Public Property allowedValues As List(Of String)
        Public Property blocked As Boolean
    End Class

    Public Class FilterFilterImportadorCnpj
        Public Property allowedValues As List(Of String)
        Public Property blocked As Boolean
    End Class

    Public Class FilterPossibleImporterCityFilter
        Public Property allowedValues As List(Of String)
        Public Property blocked As Boolean
    End Class

    Public Class FilterSiglaUfImporter
        Public Property allowedValues As List(Of String)
        Public Property blocked As Boolean
    End Class

    Public Class FilterDscDepto
        Public Property allowedValues As List(Of String)
        Public Property blocked As Boolean
    End Class

    Public Class FilterProduct
        Public Property allowedValues As List(Of String)
        Public Property blocked As Boolean
    End Class

    Public Class FilterInformationNatureType
        Public Property allowedValues As List(Of String)
        Public Property blocked As Boolean
    End Class

    Public Class FilterFilterValorFobUnit
        Public Property allowedValues As List(Of String)
        Public Property blocked As Boolean
    End Class

    Public Class FiltersSchemas
        Public Property filterNcm As FilterNcm
        Public Property filterPeriod As FilterPeriod
        Public Property filterIncoterm As FilterIncoterm
        Public Property filterOriginCountry As FilterOriginCountry
        Public Property filterPossibleImporter As FilterPossibleImporter
        Public Property filterPossibleExporter As FilterPossibleExporter
        Public Property filterAcquisitionCountry As FilterAcquisitionCountry
        Public Property filterProductDescription As FilterProductDescription
        Public Property filterClearanceUnitName As FilterClearanceUnitName
        Public Property filterStaticUnit As FilterStaticUnit
        Public Property filterTransportRouteType As FilterTransportRouteType
        Public Property filterPossibleNotified As FilterPossibleNotified
        Public Property filterPossibleManufactor As FilterPossibleManufactor
        Public Property filterFilterImportadorCnpj As FilterFilterImportadorCnpj
        Public Property filterPossibleImporterCityFilter As FilterPossibleImporterCityFilter
        Public Property filterSiglaUfImporter As FilterSiglaUfImporter
        Public Property filterDscDepto As FilterDscDepto
        Public Property filterProduct As FilterProduct
        Public Property filterInformationNatureType As FilterInformationNatureType
        Public Property filterFilterValorFobUnit As FilterFilterValorFobUnit
    End Class

    Public Class RangeLimits
        Public Property min As Double
        Public Property max As Double
    End Class

    Public Class RangeValues
        Public Property min As Double
        Public Property max As Double
        Public Property filterName As String
        Public Property rangeLimits As RangeLimits
        Public Property rangeValues As RangeValues
    End Class

    Public Class ComexStat
        Public Property startDate As String
        Public Property companies As List(Of Integer)
    End Class

    Public Class Layout
        Public Property excelHasLinesToDownload As Boolean
        Public Property excelHasDownloads As Boolean
        Public Property showAlertOfSearchLinesLimit As Boolean
        Public Property totalHitsWithoutSearchLinesLimit As Integer
        Public Property showPossibleGuysFilterLimited As Boolean
        Public Property userHasPermissionToUseExcel As Boolean
        Public Property filtersSchemas As FiltersSchemas
        Public Property noCopyDetails As Boolean
        Public Property rangeValues As List(Of RangeValues)
        Public Property comexStat As ComexStat
    End Class

    Public Class Session
        Public Property customer As Customer
        Public Property user As User
        Public Property plan As Plan
        Public Property products As List(Of Product)
        Public Property productsStatus As List(Of ProductsStatu)
        Public Property layout As Layout
    End Class

    <Serializable> Public Class clsDadosFichas
        Public Property data As Ficha
        Public Property session As Session
    End Class



End Namespace
