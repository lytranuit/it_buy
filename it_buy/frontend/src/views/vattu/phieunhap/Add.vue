<template>
    <div class="row clearfix">
        <div class="col-12">
            <form method="POST" id="form">
                <section class="card card-fluid">
                    <div class="card-body">
                        <div class="row">
                            <div class="col-md-4">
                                <div class="form-group row">
                                    <b class="col-12 col-lg-12 col-form-label">Số phiếu:<i class="text-danger">*</i></b>
                                    <div class="col-12 col-lg-12 pt-1">
                                        <input class="form-control" v-model="model.sohd" :disabled="true" />
                                    </div>
                                </div>
                            </div>
                            <div class="col-md-4">
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
                            <div class="col-md-4">
                                <div class="form-group row">
                                    <b class="col-12 col-lg-12 col-form-label">
                                        Mã kho
                                        <i class="text-danger">*</i>
                                    </b>
                                    <div class="col-12 col-lg-12 pt-1">
                                        <KhoTreeSelect v-model="model.makho" :multiple="false" :only_access="true">
                                        </KhoTreeSelect>
                                    </div>
                                </div>
                            </div>
                            <div class="col-md-12">
                                <div class="form-group row">
                                    <b class="col-12 col-lg-12 col-form-label">Ghi chú:</b>
                                    <div class="col-12 col-lg-12 pt-1">
                                        <textarea class="form-control form-control-sm" v-model="model.note"
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
                            <div class="col-md-12 text-center">
                                <Button label="Lưu lại" icon="pi pi-save" class="p-button-success p-button-sm mr-2"
                                    @click="submit()" :disabled="buttonDisabled"></Button>
                            </div>
                        </div>
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
import Form from "../../../components/vattu/phieunhap/Form.vue";
import KhoTreeSelect from "../../../components/TreeSelect/KhoTreeSelect.vue";
const toast = useToast();
const minDate = new Date();
const route = useRoute();
const storeVattu = useVattu();
const router = useRouter();
const messageError = ref();
const store = useAuth();
const store_general = useGeneral();
const buttonDisabled = ref();
const { model, datatable, list_add } = storeToRefs(storeVattu);
const { user } = storeToRefs(store);
onMounted(() => {
    store_general.fetchMaterials();
    if (!route.query.no_reset) {
        storeVattu.reset();

        model.value.ngaylap = new Date();
        model.value.makho = user.value.warehouses_vt.length > 0 ? user.value.warehouses_vt[0] : null;
        datatable.value.push({
            ids: rand(), stt: 1, soluong: 1,
            dvt: '',
        });
    }
});
const submit = () => {
    buttonDisabled.value = true;

    if (!model.value.ngaylap) {
        alert("Chưa nhập ngày nhập!");
        buttonDisabled.value = false;
        return false;
    }
    if (!model.value.makho) {
        alert("Chưa chọn kho!");
        buttonDisabled.value = false;
        return false;
    }
    if (datatable.value.length) {
        for (let product of datatable.value) {
            if (!product.mahh) {
                alert("Chưa nhập hàng hóa!");
                buttonDisabled.value = false;
                return false;
            }

            if (!product.dvt) {
                alert("Chưa nhập đơn vị tính!");
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

    var params = model.value;
    VattuApi.saveNhap(params).then((response) => {
        messageError.value = "";
        if (response.success) {
            toast.add({
                severity: "success",
                summary: "Thành công!",
                detail: "Tạo dự trù thành công",
                life: 3000,
            });
            router.push("/Vattu/phieunhap");
        } else {
            messageError.value = response.message;
        }
        // console.log(response)
    });
};
</script>