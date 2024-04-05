<template>
    <div id="TableDutruChitiet">
        <DataTable showGridlines :value="datatable" ref="dt" class="p-datatable-ct" :rowHover="true" :loading="loading"
            responsiveLayout="scroll" :resizableColumns="true" columnResizeMode="expand" v-model:selection="selected">
            <template #header>
                <div class="d-inline-flex" style="width:200px" v-if="model.status_id == 1">
                    <Button label="Thêm" icon="pi pi-plus" class="p-button-success p-button-sm mr-2"
                        @click="addRow"></Button>
                    <Button label="Xóa" icon="pi pi-trash" class="p-button-danger p-button-sm"
                        :disabled="!selected || !selected.length" @click="confirmDeleteSelected"></Button>

                </div>
                <div class="d-inline-flex float-right">
                </div>
            </template>

            <template #empty>
                <div class='text-center'>Không có dữ liệu.</div>
            </template>
            <Column selectionMode="multiple" v-if="model.status_id == 1"></Column>
            <Column v-for="(col, index) in selectedColumns" :field="col.data" :header="col.label" :key="col.data"
                :showFilterMatchModes="false" :class="col.data">

                <template #body="slotProps">

                    <template v-if="col.data == 'tenhh' && model.status_id == 1">
                        <div class="d-flex align-items-center">
                            <MaterialAutoComplete v-model="slotProps.data[col.data]"
                                :disabled="slotProps.data['hh_id'] != null" :type_id="model.type_id"
                                @item-select="select($event, slotProps.data)">
                            </MaterialAutoComplete>
                            <!-- <span class="fas fa-plus ml-3 text-success" style="cursor: pointer;"
                                @click="openNew(slotProps.index)" v-if="model.type_id != 1"></span> -->
                        </div>
                    </template>

                    <template
                        v-else-if="(col.data == 'dvt' || col.data == 'dangbaoche' || col.data == 'grade' || col.data == 'tensp' || col.data == 'masothietke') && model.status_id == 1">
                        <input v-model="slotProps.data[col.data]" class="p-inputtext p-inputtext-sm" required />
                    </template>
                    <template v-else-if="col.data == 'nhasx' && model.status_id == 1">
                        <NsxTreeSelect v-model="slotProps.data['mansx']" :required="true" :useID="false"
                            @update:modelValue="changeProducer(slotProps.data)">
                        </NsxTreeSelect>
                    </template>
                    <template v-else-if="col.data == 'soluong' && model.status_id == 1">
                        <InputNumber v-model="slotProps.data[col.data]" class="p-inputtext-sm" :maxFractionDigits="2" />
                    </template>

                    <template v-else-if="col.data == 'note' && model.status_id == 1">
                        <textarea v-model="slotProps.data[col.data]" class="form-control" />
                    </template>

                    <template v-else-if="col.data == 'dinhkem' && model.status_id == 1">
                        <div class="custom-file mt-2">
                            <input type="file" class="hinhanh-file-input" :id="'hinhanhFile' + slotProps.data['stt']"
                                :multiple="true" :data-key="slotProps.data['stt']" @change="fileChange($event)">
                            <label class="custom-file-label" :for="'hinhanhFile' + slotProps.data['stt']">Choose
                                file</label>
                        </div>
                        <div class="mt-2 dinhkemchitiet" v-for="(item, index) in slotProps.data['dinhkem']"
                            :key="index">
                            <a target="_blank" :href="item.url" class="text-blue" :download="download(item.name)">{{
            item.name
        }}</a><i class="text-danger fas fa-trash ml-2" style="cursor: pointer;"
                                @click="xoachitietdinhkem(index, slotProps.data['dinhkem'])"></i>
                        </div>
                    </template>

                    <template v-else-if="col.data == 'dinhkem'">
                        <div class="mt-2 dinhkemchitiet" v-for="(item, index) in slotProps.data['dinhkem']"
                            :key="index">
                            <a target="_blank" :href="item.url" class="text-blue" :download="download(item.name)">{{
            item.name
        }}</a>
                        </div>
                    </template>

                    <template v-else-if="col.data == 'tenhh' && model.status_id != 1">
                        {{ label(slotProps.data) }}
                    </template>

                    <template v-else>
                        {{ slotProps.data[col.data] }}
                    </template>
                </template>
            </Column>
        </DataTable>
    </div>
</template>

<script setup>

import { onMounted, ref, watch, computed } from 'vue';
import Materials from "../../components/TreeSelect/MaterialTreeSelect.vue"
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Button from 'primevue/button';
import InputText from 'primevue/inputtext';
import InputNumber from 'primevue/inputnumber';
import { storeToRefs } from 'pinia'

import { useConfirm } from "primevue/useconfirm";
import { rand } from '../../utilities/rand'
import { useDutru } from '../../stores/dutru';
import { useGeneral } from '../../stores/general';
import { useMaterials } from '../../stores/materials';
import NsxTreeSelect from '../TreeSelect/NsxTreeSelect.vue';
import MaterialAutoComplete from '../AutoComplete/MaterialAutoComplete.vue';
import { download } from '../../utilities/util';
import dutruApi from '../../api/dutruApi';
import { useToast } from 'primevue/usetoast';
const toast = useToast();
const fileChange = (e) => {
    var parents = $(e.target).parents(".custom-file");
    var label = $(".custom-file-label", parents);
    label.text(e.target.files.length + " Files");
}

const xoachitietdinhkem = (index, item) => {
    // console.log(item.dinhkem[key1]);
    confirm.require({
        message: 'Bạn có chắc muốn xóa?',
        header: 'Xác nhận',
        icon: 'pi pi-exclamation-triangle',
        accept: () => {

            // waiting.value = true;
            dutruApi.xoachitietdinhkem({ id: item[index].id }).then((response) => {
                // waiting.value = false;
                if (response.success) {
                    toast.add({ severity: 'success', summary: 'Thành công!', detail: 'Xóa đính kèm', life: 3000 });
                    item.splice(index, 1);
                }

                // console.log(response)
            });


        }
    });

}

const label = (row) => {
    return row.mahh ? row.mahh + ' - ' + row.tenhh : row.tenhh;
}
const select = (event, row) => {
    console.log(event)
    var id = event.value.id;
    row.hh_id = "m-" + id
    store_general.changeMaterial(row);
    // console.log(row)
}
const store_dutru = useDutru();
const store_general = useGeneral();
const store_materials = useMaterials();
const RefMaterials = storeToRefs(store_materials);
const modelMaterial = RefMaterials.model;
const { headerForm, visibleDialog } = RefMaterials;

const { datatable, model, list_delete } = storeToRefs(store_dutru);
const { materials } = storeToRefs(store_general);
const changeMaterial = store_general.changeMaterial;
const changeProducer = store_general.changeProducer;
const confirm = useConfirm();

const loading = ref(false);
const selected = ref();
const columns = computed(() => {
    if (model.value.type_id == 1) {
        return [
            {
                label: "STT(*)",
                data: "stt",
                className: "text-center",
            },
            {
                label: "Hàng hóa(*)",
                "data": "tenhh",
                "className": "text-center",
            },
            // {
            //     label: "Tên(*)",
            //     "data": "tenhh",
            //     "className": "text-center"
            // },
            {
                label: "Grade(*)",
                "data": "grade",
                "className": "text-center"
            }, {
                label: "Tên SP(*)",
                "data": "tensp",
                "className": "text-center"
            }, {
                label: "Dạng bào chế(*)",
                "data": "dangbaoche",
                "className": "text-center"
            },
            {
                label: "ĐVT(*)",
                "data": "dvt",
                "className": "text-center",
            },
            {
                label: "Số lượng(*)",
                "data": "soluong",
                "className": "text-center"
            }, {
                label: "Mã Artwork",
                "data": "masothietke",
                "className": "text-center"
            }, {
                label: "Nhà sản xuất",
                "data": "nhasx",
                "className": "text-center"
            },
            {
                label: "Mô tả",
                "data": "note",
                "className": "text-center",
            }
        ]
    } else {
        return [
            {
                label: "STT(*)",
                data: "stt",
                className: "text-center",
            },
            {
                label: "Hàng hóa(*)",
                "data": "tenhh",
                "className": "text-center",
            },
            {
                label: "ĐVT(*)",
                "data": "dvt",
                "className": "text-center",
            },
            {
                label: "Số lượng(*)",
                "data": "soluong",
                "className": "text-center"
            },
            {
                label: "Mô tả",
                "data": "note",
                "className": "text-center",
            },
            {
                label: "Hình ảnh",
                "data": "dinhkem",
                "className": "text-center",
            }
        ]
    }
});
const indexActive = ref();
const selectedColumns = computed(() => {
    return columns.value.filter(col => col.hide != true);
});
const addRow = () => {
    let stt = 0;
    if (datatable.value.length) {
        stt = datatable.value[datatable.value.length - 1].stt;
    }
    stt++;
    datatable.value.push({ ids: rand(), stt: stt, soluong: 1 })
}
const confirmDeleteSelected = () => {
    confirm.require({
        message: 'Bạn có chắc muốn xóa những dòng đã chọn?',
        header: 'Xác nhận',
        icon: 'pi pi-exclamation-triangle',
        accept: () => {
            datatable.value = datatable.value.filter((item) => {
                return !selected.value.includes(item)
            });

            if (!list_delete.value) {
                list_delete.value = [];
            }
            for (var item of selected.value) {
                if (!item.ids) {
                    list_delete.value.push(item);
                }
            }
            selected.value = [];
            // selected


        }
    });
}
onMounted(async () => {
    await store_general.fetchMaterials();

    // if(items)
})
</script>

<style>
.hh_id {
    min-width: 300px;
}

.tenhh {
    min-width: 300px;
}
</style>