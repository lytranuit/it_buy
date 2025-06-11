<template>
    <div class="row clearfix">
        <div class="col-12">
            <form method="POST" id="form">
                <section class="card card-fluid">
                    <div class="card-body">
                        <div class="row">
                            <div class="col-md">
                                <div class="form-group row">
                                    <b class="col-12 col-lg-12 col-form-label">Số phiếu:<i class="text-danger">*</i></b>
                                    <div class="col-12 col-lg-12 pt-1">
                                        <input class="form-control" v-model="model.sohd" :disabled="true" />
                                    </div>
                                </div>
                            </div>
                            <div class="col-md">
                                <div class="form-group row">
                                    <b class="col-12 col-lg-12 col-form-label">
                                        Ngày nhập:
                                        <i class="text-danger">*</i>
                                    </b>
                                    <div class="col-12 col-lg-12 pt-1">
                                        <Calendar v-model="model.ngaylap" dateFormat="yy-mm-dd" class="date-custom"
                                            :manualInput="false" showIcon />
                                    </div>
                                </div>
                            </div>
                            <div class="col-md">
                                <div class="form-group row">
                                    <b class="col-12 col-lg-12 col-form-label">
                                        Từ kho
                                        <i class="text-danger">*</i>
                                    </b>
                                    <div class="col-12 col-lg-12 pt-1">
                                        <KhoTreeSelect v-model="model.noidi" :multiple="false" :only_access="true"
                                            @update:modelValue="changeNoidi">
                                        </KhoTreeSelect>
                                    </div>
                                </div>
                            </div>
                            <div class="col-md">
                                <div class="form-group row">
                                    <b class="col-12 col-lg-12 col-form-label">
                                        Đến kho
                                        <i class="text-danger">*</i>
                                    </b>
                                    <div class="col-12 col-lg-12 pt-1">
                                        <KhoTreeSelect v-model="model.noiden" :multiple="false">
                                        </KhoTreeSelect>
                                    </div>
                                </div>
                            </div>
                            <div class="col-md">
                                <div class="form-group row">
                                    <b class="col-12 col-lg-12 col-form-label">
                                        Loại hàng hóa
                                        <i class="text-danger">*</i>
                                    </b>
                                    <div class="col-12 col-lg-12 pt-1">
                                        <PLHHTreeSelect v-model="model.pl" :multiple="false">
                                        </PLHHTreeSelect>
                                    </div>
                                </div>
                            </div>
                            <div class="col-md-12">
                                <div class="form-group row">
                                    <b class="col-12 col-lg-12 col-form-label">Ghi chú:</b>
                                    <div class="col-12 col-lg-12 pt-1">
                                        <textarea class="form-control form-control-sm" v-model="model.ghichu"
                                            required></textarea>
                                    </div>
                                </div>
                                <div class="form-group row">
                                    <b class="col-12 col-lg-12 col-form-label">Hàng hóa:<i class="text-danger">*</i></b>
                                    <div class="col-12 col-lg-12 pt-1">
                                        <Form></Form>
                                    </div>
                                </div>
                            </div>

                        </div>
                    </div>
                    <div class="card-footer text-center">
                        <Button label="Lưu lại" icon="pi pi-save" class="p-button-success p-button-sm mr-2"
                            @click="submit()" :disabled="buttonDisabled"></Button>
                    </div>
                </section>
            </form>
        </div>
    </div>
</template>

<script setup>
import { ref } from "vue";
import { onMounted, computed } from "vue";
import { useAuth } from "../../../stores/auth";

import Button from "primevue/button";
import Calendar from "primevue/calendar";
import { useRoute, useRouter } from "vue-router";
import VattuApi from "../../../api/VattuApi";
import { useVattu } from "../../../stores/vattu";
import { storeToRefs } from "pinia";
import { useToast } from "primevue/usetoast";
import { rand } from "../../../utilities/rand";
import { useGeneral } from "../../../stores/general";
import Form from "../../../components/vattu/phieuxuat/Form.vue";
import KhoTreeSelect from "../../../components/TreeSelect/KhoTreeSelect.vue";
import PLHHTreeSelect from "../../../components/TreeSelect/PLHHTreeSelect.vue";
const toast = useToast();
const minDate = new Date();
const route = useRoute();
const storeVattu = useVattu();
const router = useRouter();
const messageError = ref();
const store = useAuth();
const store_general = useGeneral();
const buttonDisabled = ref();
const { model, datatable, list_add, list_delete, list_update, start_event } = storeToRefs(storeVattu);
const { user } = storeToRefs(store);

onMounted(() => {
    store_general.fetchMaterials();
    if (!route.query.no_reset) {
        storeVattu.reset();
        model.value.ngaylap = new Date();
        model.value.noidi = user.value.warehouses_vt.length > 0 ? user.value.warehouses_vt[0] : null;
        datatable.value.push({
            ids: rand(),
            stt: 1,
            kt_xuat: true,
            dvt: '',
        });
    }

    start_event.value = true;
});
const changeNoidi = (value) => {
    datatable.value = [];

    if (!list_delete.value) {
        list_delete.value = [];
    }
    for (var item of datatable.value) {
        if (!item.ids) {
            list_delete.value.push(item);
        }
    }
};
const submit = () => {
    buttonDisabled.value = true;

    if (!model.value.ngaylap) {
        alert("Chưa nhập ngày nhập!");
        buttonDisabled.value = false;
        return false;
    }
    if (!model.value.noidi) {
        alert("Chưa chọn kho xuất!");
        buttonDisabled.value = false;
        return false;
    }
    if (!model.value.noiden) {
        alert("Chưa chọn kho nhập!");
        buttonDisabled.value = false;
        return false;
    }
    if (datatable.value.length) {
        for (let product of datatable.value) {
            if (!product.tenhh) {
                alert("Chưa nhập hàng hóa!");
                buttonDisabled.value = false;
                return false;
            }

            if (!(product.soluong > 0)) {
                alert("Chưa chọn nhập số lượng");
                buttonDisabled.value = false;
                return false;
            }
        }
    } else {
        alert("Chưa nhập hàng hóa!");
        buttonDisabled.value = false;
        return false;
    }
    model.value.list_add = list_add.value;
    model.value.list_update = list_update.value;
    model.value.list_delete = list_delete.value;

    var params = model.value;
    VattuApi.saveXuat(params).then((response) => {
        messageError.value = "";
        if (response.success) {
            toast.add({
                severity: "success",
                summary: "Thành công!",
                detail: "Tạo dự trù thành công",
                life: 3000,
            });
            router.push("/Vattu/phieuxuat");
        } else {
            messageError.value = response.message;
        }
        // console.log(response)
    });
};
</script>