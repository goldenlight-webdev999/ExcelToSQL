
/****** Object:  StoredProcedure [dbo].[STP_Util_PDSUpdate_V2]    Script Date: 9/13/2020 8:09:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER procedure [dbo].[STP_Util_PDSUpdate_V2] (@period char(5)) as
-- Update PDS Data from TempPDS table
begin
-- Under Development ??


--Add missing UPCs first to PDS
insert into pds (upc) select upc from temppds where upc not in(select upc from pds)
--update Lastupdt for these records
update pds set lastupdt=getdate(),
prod_desc=case when isnull(t.prod_desc,'')<>''	then t.prod_desc else p.prod_desc end,  
cp_wt    =case when isnull(t.cp_wt,'')<>''		then t.cp_wt else p.cp_wt end,  
h_importc=case when isnull(t.country,'')<>''	then t.country else p.h_importc end,  
mfgitem  =case when isnull(t.mfgitem,'')<>''	then t.mfgitem else p.mfgitem end,  
sugg_retl=case when isnull(t.sugg_retl,'')<>''	then t.sugg_retl else p.sugg_retl end,  
sugg_deal=case when isnull(t.sugg_deal,'')<>''	then t.sugg_deal else p.sugg_deal end,  
dist_ea  =case when isnull(t.dist_ea,'')<>''		then t.dist_ea else p.dist_ea end,  
dropship =case when isnull(t.dropship,'')<>''		then t.dropship else p.dropship end,  
driveitem=case when isnull(t.driveitem,'')<>''	then t.driveitem else p.driveitem end,  
price_a  =case when isnull(t.price_a,'')<>''		then t.price_a else p.price_a end,  
price_b  =case when isnull(t.price_b,'')<>''		then t.price_b else p.price_b end,  
price_c  =case when isnull(t.price_c,'')<>''		then t.price_c else p.price_c end,  
price_d  =case when isnull(t.price_d,'')<>''		then t.price_d else p.price_d end,  
price_e  =case when isnull(t.price_e,'')<>''		then t.price_e else p.price_e end,  
eff_date  =case when t.eff_date is null			then p.eff_date else t.eff_date end,  
cp_upc    =case when isnull(t.cp_upc,'')<>''	then t.cp_upc else p.cp_upc end,  
cp_ht  =case when isnull(t.cp_ht,'')<>''			then t.cp_ht else p.cp_ht end,  
cp_cube  =case when isnull(t.cp_cube,'')<>''		then t.cp_cube else p.cp_cube end,  
cp_WTH  =case when isnull(t.cp_WTH,'')<>''		then t.cp_WTH else p.cp_WTH end,  
cp_dpth  =case when isnull(t.cp_dpth,'')<>''		then t.cp_dpth else p.cp_dpth end,  
cp_palqty  =case when isnull(t.cp_palqty,'')<>''	then t.cp_palqty else p.cp_palqty end,
cp_qty  =case when isnull(t.cp_qty,'')<>''		then t.cp_qty else p.cp_qty end,  
h_msdsreq  =case when isnull(t.h_msdsreq,'')<>''	then t.h_msdsreq else p.h_msdsreq end,  
MP_qty  =case when isnull(t.MP_qty,'')<>''		then t.MP_qty else p.MP_qty end,  
country  =case when isnull(t.country,'')<>''	then t.country else p.country end ,
f01H_distCode  =case when isnull(t.f01H_distCode,'')<>''	then t.f01H_distCode else p.f01H_distCode end,
f01J_prodcDesc2  =case when isnull(t.f01J_prodcDesc2,'')<>''	then t.f01J_prodcDesc2 else p.f01J_prodcDesc2 end,
f01M_packTier  =case when isnull(t.f01M_packTier,'')<>''	then t.f01M_packTier else p.f01M_packTier end,
f01N_tierPit  =case when isnull(t.f01N_tierPit,'')<>''	then t.f01N_tierPit else p.f01N_tierPit end,
f01V_itemHeightEach  =case when isnull(t.f01V_itemHeightEach,'')<>''	then t.f01V_itemHeightEach else p.f01V_itemHeightEach end,
f01W_itemWidthEach  =case when isnull(t.f01W_itemWidthEach,'')<>''	then t.f01W_itemWidthEach else p.f01W_itemWidthEach end,
f01X_itemDepthEach  =case when isnull(t.f01X_itemDepthEach,'')<>''	then t.f01X_itemDepthEach else p.f01X_itemDepthEach end,

f01Y_itemCubicFeetEach  =case when isnull(t.f01Y_itemCubicFeetEach,'')<>''	then t.f01Y_itemCubicFeetEach else p.f01Y_itemCubicFeetEach end,
f01Z_itemWeightEach  =case when isnull(t.f01Z_itemWeightEach,'')<>''	then t.f01Z_itemWeightEach else p.f01Z_itemWeightEach end,
f01AA_heightPurchasingUOM  =case when isnull(t.f01AA_heightPurchasingUOM,'')<>''	then t.f01AA_heightPurchasingUOM else p.f01AA_heightPurchasingUOM end,
f01AB_widthPurchasingUOM  =case when isnull(t.f01AB_widthPurchasingUOM,'')<>''	then t.f01AB_widthPurchasingUOM else p.f01AB_widthPurchasingUOM end,


f01AC_depthPurchasingUOM  =case when isnull(t.f01AC_depthPurchasingUOM,'')<>''	then t.f01AC_depthPurchasingUOM else p.f01AC_depthPurchasingUOM end,
f01AD_cubicFeePurchasingUOM  =case when isnull(t.f01AD_cubicFeePurchasingUOM,'')<>''	then t.f01AD_cubicFeePurchasingUOM else p.f01AD_cubicFeePurchasingUOM end,
f01AE_weightFeePurchasingUOM  =case when isnull(t.f01AE_weightFeePurchasingUOM,'')<>''	then t.f01AE_weightFeePurchasingUOM else p.f01AE_weightFeePurchasingUOM end,
f01AF_purchasingUOMQty  =case when isnull(t.f01AF_purchasingUOMQty,'')<>''	then t.f01AF_purchasingUOMQty else p.f01AF_purchasingUOMQty end,
f01AG_purchasingUOMCode  =case when isnull(t.f01AG_purchasingUOMCode,'')<>''	then t.f01AG_purchasingUOMCode else p.f01AG_purchasingUOMCode end,


f01AH_packCuFt  =case when isnull(t.f01AH_packCuFt,'')<>''	then t.f01AH_packCuFt else p.f01AH_packCuFt end,
f01AI_packWeight  =case when isnull(t.f01AI_packWeight,'')<>''	then t.f01AI_packWeight else p.f01AI_packWeight end,
f01AJ_packWidth  =case when isnull(t.f01AJ_packWidth,'')<>''	then t.f01AJ_packWidth else p.f01AJ_packWidth end,
f01AK_packDepth  =case when isnull(t.f01AK_packDepth,'')<>''	then t.f01AK_packDepth else p.f01AK_packDepth end,
f01AL_packHeigth  =case when isnull(t.f01AL_packHeigth,'')<>''	then t.f01AL_packHeigth else p.f01AL_packHeigth end,

f01AM_tihiBottomLayerQty  =case when isnull(t.f01AM_tihiBottomLayerQty,'')<>''	then t.f01AM_tihiBottomLayerQty else p.f01AM_tihiBottomLayerQty end,
f01AN_tihiRowHighQty1  =case when isnull(t.f01AN_tihiRowHighQty1,'')<>''	then t.f01AN_tihiRowHighQty1 else p.f01AN_tihiRowHighQty1 end,
f01AO_tihiBottomLayerType  =case when isnull(t.f01AO_tihiBottomLayerType,'')<>''	then t.f01AO_tihiBottomLayerType else p.f01AO_tihiBottomLayerType end,
f01AP_packTier  =case when isnull(t.f01AP_packTier,'')<>''	then t.f01AP_packTier else p.f01AP_packTier end,
f01AQ_tierPLT  =case when isnull(t.f01AQ_tierPLT,'')<>''	then t.f01AQ_tierPLT else p.f01AQ_tierPLT end,


f01AR_epaRegulationCode  =case when isnull(t.f01AR_epaRegulationCode,'')<>''	then t.f01AR_epaRegulationCode else p.f01AR_epaRegulationCode end,
f01AS_insuranceClassCode  =case when isnull(t.f01AS_insuranceClassCode,'')<>''	then t.f01AS_insuranceClassCode else p.f01AS_insuranceClassCode end,
f01AT_hazardAirCode  =case when isnull(t.f01AT_hazardAirCode,'')<>''	then t.f01AT_hazardAirCode else p.f01AT_hazardAirCode end,
f01AU_hazardWaterCode  =case when isnull(t.f01AU_hazardWaterCode,'')<>''	then t.f01AU_hazardWaterCode else p.f01AU_hazardWaterCode end,
f01AV_hazardGroundCode  =case when isnull(t.f01AV_hazardGroundCode,'')<>''	then t.f01AV_hazardGroundCode else p.f01AV_hazardGroundCode end,

f01AW_lotControllerItem  =case when isnull(t.f01AW_lotControllerItem,'')<>''	then t.f01AW_lotControllerItem else p.f01AW_lotControllerItem end,
f01AX_frtClass  =case when isnull(t.f01AX_frtClass,'')<>''	then t.f01AX_frtClass else p.f01AX_frtClass end,
f01AY_usHamonizedTariffNbr  =case when isnull(t.f01AY_usHamonizedTariffNbr,'')<>''	then t.f01AY_usHamonizedTariffNbr else p.f01AY_usHamonizedTariffNbr end,
f01AZ_hts301TariffNbr  =case when isnull(t.f01AZ_hts301TariffNbr,'')<>''	then t.f01AZ_hts301TariffNbr else p.f01AZ_hts301TariffNbr end,
f01BA_msdsDocDate  =case when isnull(t.f01BA_msdsDocDate,'')<>''	then t.f01BA_msdsDocDate else p.f01BA_msdsDocDate end,


f01BD_listPrice  =case when isnull(t.f01BD_listPrice,'')<>''	then t.f01BD_listPrice else p.f01BD_listPrice end,
f01BH_dealerDropShip  =case when isnull(t.f01BH_dealerDropShip,'')<>''	then t.f01BH_dealerDropShip else p.f01BH_dealerDropShip end,
f01BI_distributionImportFOB  =case when isnull(t.f01BI_distributionImportFOB,'')<>''	then t.f01BI_distributionImportFOB else p.f01BI_distributionImportFOB end,
f01BJ_distributionDomesticFOB  =case when isnull(t.f01BJ_distributionDomesticFOB,'')<>''	then t.f01BJ_distributionDomesticFOB else p.f01BJ_distributionDomesticFOB end,
f01BR_mItemNbr  =case when isnull(t.f01BR_mItemNbr,'')<>''	then t.f01BR_mItemNbr else p.f01BR_mItemNbr end,

f01BS_eanNbr  =case when isnull(t.f01BS_eanNbr,'')<>''	then t.f01BS_eanNbr else p.f01BS_eanNbr end,
f01BU_imageAvailable  =case when isnull(t.f01BU_imageAvailable,'')<>''	then t.f01BU_imageAvailable else p.f01BU_imageAvailable end,
f01BV_imageFormat  =case when isnull(t.f01BV_imageFormat,'')<>''	then t.f01BV_imageFormat else p.f01BV_imageFormat end,
f01BW_productShelfLine  =case when isnull(t.f01BW_productShelfLine,'')<>''	then t.f01BW_productShelfLine else p.f01BW_productShelfLine end,
f01BX_P65  =case when isnull(t.f01BX_P65,'')<>''	then t.f01BX_P65 else p.f01BX_P65 end,
f01BY_SDS  =case when isnull(t.f01BY_SDS,'')<>''	then t.f01BY_SDS else p.f01BY_SDS end,
f01BZ_waHaz  =case when isnull(t.f01BZ_waHaz,'')<>''	then t.f01BZ_waHaz else p.f01BZ_waHaz end,
f01CA_califirniaRegNbr  =case when isnull(t.f01CA_califirniaRegNbr,'')<>''	then t.f01CA_califirniaRegNbr else p.f01CA_califirniaRegNbr end,
f01A_itemType  =case when isnull(t.f01A_itemType,'')<>''	then t.f01A_itemType else p.f01A_itemType end


from tempPDS t left join pds p on t.upc=p.upc where p.upc is not null
--from pds p left join tempPDS t on t.upc=p.upc where p.upc is not null

insert into Product (mfgno ,Oldmfgno, upc ,mfgpartno,proddesc, f01J_prodcDesc2, cp_qty, f01A_itemType ) select mfgno,oldmfgno,upc,mfgitem,prod_desc, f01J_prodcDesc2, cp_qty, f01A_itemType from temppds where upc not in(select upc from Product)


Update Product set pds_date = getdate(), 
f01A_itemType  = case when isnull(t.f01A_itemType,'') <> '' then t.f01A_itemType else p.f01A_itemType end,
cp_qty  =case when isnull(t.cp_qty,'')<>''		then t.cp_qty else p.cp_qty end
from tempPDS t left join Product P  on t.upc=p.upc where p.upc is not null

end
