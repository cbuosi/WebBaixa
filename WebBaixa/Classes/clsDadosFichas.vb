Option Explicit On
Option Strict On

Namespace DadosFichas

    <Serializable> Public Class Ficha
        Public Property nm_pais_origem As String
        Public Property qtd_comerc As String
        Public Property importador_endereco As String
        Public Property nm_pais_aquis As String
        Public Property via_transp As String
        Public Property nome_unid_desembaraco As String
        Public Property anomes As String
        Public Property id_import As String
        Public Property val_fob_us_subitem As String
        Public Property exportador_nome As String
        Public Property desc_prodt As String
        Public Property tp_unid_comerc As String
        Public Property incoterm As String
        Public Property importador_cnpj As String
        Public Property val_cif_un_us As String
        Public Property val_fob_un_us As String
        Public Property val_vmld_us_subitem As String
        Public Property cdncm_compl As String
        Public Property val_peso_liq_subitem As String
        Public Property importador_nome As String
        Public Property nome_adquirente As String
        Public Property val_frete_un_us As String
        Public Property val_frete_us_subitem As String
        Public Property val_seg_un_us As String
        Public Property val_seg_us_subitem As String
        Public Property cidade_import As String
    End Class

    <Serializable> Public Class Data
        Public Property ficha As List(Of Ficha)
        Public Property scrollHash As String
        Public Property scrollTimeout As Integer
        Public Property itemsPerPage As Integer
        Public Property totalItems As Integer
        Public Property lastPage As Integer
        Public Property fromItem As Integer
        Public Property toItem As Integer
    End Class

    <Serializable> Public Class Customization
        Public Property customHexColor As Object
        Public Property customName As Object
        Public Property customLogo As Object
    End Class

    <Serializable> Public Class Customer
        Public Property id As Integer
        Public Property name As String
        Public Property columnRestrictions As List(Of Object)
        Public Property customization As Customization
    End Class

    <Serializable> Public Class Permission
        Public Property id As Integer
        Public Property name As String
        Public Property description As String
        Public Property slug As String
        Public Property dependency As Object
    End Class

    <Serializable> Public Class User
        Public Property id As Integer
        Public Property email As String
        Public Property name As String
        Public Property avatar As String
        Public Property permissions As List(Of Permission)
        Public Property language As String
    End Class

    <Serializable> Public Class Info
        Public Property apiAccess As Boolean
        Public Property inTrial As Boolean
        Public Property useBiConditionFilter As Boolean
        Public Property tagsForFilter As Boolean
        Public Property taxesFunctionality As Boolean
        Public Property excelExport As Boolean
        Public Property excelLines As Integer
        Public Property excelDownloads As Integer
        Public Property monthlySearches As Integer
        Public Property searchDaysLimit As Integer
        Public Property searchDaysRange As Integer
        Public Property searchLinesLimit As Integer
        Public Property userLimit As Integer
        Public Property filterPossibleGuysLimit As Integer
        Public Property trialRemainingDays As Integer
        Public Property trialStart As Object
        Public Property trialExpiration As Object
        Public Property contractStart As DateTime
        Public Property contractExpiration As DateTime
        Public Property deadlineMonthNewData As Integer
        Public Property expoProductFunctionality As Boolean
        Public Property expoDashboardChartYearToDate As Boolean
        Public Property expoRealExporterFunctionality As Boolean
        Public Property expoAverageShipmentFunctionality As Boolean
    End Class

    <Serializable> Public Class Column
        Public Property name As String
        Public Property [alias] As String
        Public Property description As String
        Public Property method As String
        Public Property aliasEn As String
        Public Property dbColumnRepresentation As String
        Public Property filterName As String
    End Class

    <Serializable> Public Class Usage
        Public Property quantitySearches As Integer
        Public Property quantityExcel As Integer
        Public Property quantityFilterPossibleGuys As Integer
    End Class

    <Serializable> Public Class Plan
        Public Property info As Info
        Public Property columns As List(Of Column)
        Public Property usage As Usage
    End Class

    <Serializable> Public Class FilterNcm
        Public Property allowedValues As List(Of Object)
        Public Property blocked As Boolean
    End Class

    <Serializable> Public Class AllowedValues
        Public Property min As Integer
        Public Property max As Integer
    End Class

    <Serializable> Public Class FilterPeriod
        Public Property allowedValues As AllowedValues
        Public Property blocked As Boolean
    End Class

    <Serializable> Public Class FilterIncoterm
        Public Property allowedValues As List(Of Object)
        Public Property blocked As Boolean
    End Class

    <Serializable> Public Class FilterOriginCountry
        Public Property allowedValues As List(Of Object)
        Public Property blocked As Boolean
    End Class

    <Serializable> Public Class FilterPossibleImporter
        Public Property allowedValues As List(Of Object)
        Public Property blocked As Boolean
    End Class

    <Serializable> Public Class FilterPossibleExporter
        Public Property allowedValues As List(Of Object)
        Public Property blocked As Boolean
    End Class

    <Serializable> Public Class FilterAcquisitionCountry
        Public Property allowedValues As List(Of Object)
        Public Property blocked As Boolean
    End Class

    <Serializable> Public Class FilterProductDescription
        Public Property allowedValues As List(Of Object)
        Public Property blocked As Boolean
    End Class

    <Serializable> Public Class FilterClearanceUnitName
        Public Property allowedValues As List(Of Object)
        Public Property blocked As Boolean
    End Class

    <Serializable> Public Class FilterStaticUnit
        Public Property allowedValues As List(Of Object)
        Public Property blocked As Boolean
    End Class

    <Serializable> Public Class FilterTransportRouteType
        Public Property allowedValues As List(Of Object)
        Public Property blocked As Boolean
    End Class

    <Serializable> Public Class FilterPossibleNotified
        Public Property allowedValues As List(Of Object)
        Public Property blocked As Boolean
    End Class

    <Serializable> Public Class FilterFilterImportadorCnpj
        Public Property allowedValues As List(Of Object)
        Public Property blocked As Boolean
    End Class

    <Serializable> Public Class FilterPossibleImporterCityFilter
        Public Property allowedValues As List(Of Object)
        Public Property blocked As Boolean
    End Class

    <Serializable> Public Class FiltersSchemas
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
        Public Property filterFilterImportadorCnpj As FilterFilterImportadorCnpj
        Public Property filterPossibleImporterCityFilter As FilterPossibleImporterCityFilter
    End Class

    <Serializable> Public Class Layout
        Public Property excelHasLinesToDownload As Boolean
        Public Property excelHasDownloads As Boolean
        Public Property showAlertOfSearchLinesLimit As Boolean
        Public Property totalHitsWithoutSearchLinesLimit As Integer
        Public Property showPossibleGuysFilterLimited As Boolean
        Public Property userHasPermissionToUseExcel As Boolean
        Public Property filtersSchemas As FiltersSchemas
    End Class

    <Serializable> Public Class Session
        Public Property customer As Customer
        Public Property user As User
        Public Property plan As Plan
        Public Property products As Object
        Public Property layout As Layout
    End Class

    <Serializable> Public Class clsDadosFichas
        Public Property data As Data
        Public Property session As Session
    End Class

End Namespace
