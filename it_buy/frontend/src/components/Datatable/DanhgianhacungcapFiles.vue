<template>
    <div id="TabledanhgianhacungcapFiles">
        <DataTable showGridlines :value="files" ref="dt" class="p-datatable-ct" :rowHover="true" :loading="loading"
            responsiveLayout="scroll" :resizableColumns="true" columnResizeMode="expand" v-model:selection="selected">
            <template #header>
                <div class="d-inline-flex" style="width:200px" v-if="!model.is_chapnhan">
                    <Button label="Thêm" icon="pi pi-plus" class="p-button-success p-button-sm mr-2"
                        @click="openNew"></Button>
                </div>
                <div class="d-inline-flex float-right">
                </div>
            </template>

            <template #empty>
                <div class='text-center'>Không có dữ liệu.</div>
            </template>
            <Column v-for="(col, index) in selectedColumns" :field="col.data" :header="col.label" :key="col.data"
                :showFilterMatchModes="false" :class="col.data">

                <template #body="slotProps">

                    <template v-if="col.data == 'note'">
                        <a target="_blank" :href="slotProps.data['link']"
                            :class="{ 'text-blue': slotProps.data['link'] }">{{ slotProps.data[col.data]
                            }}</a>
                    </template>
                    <template v-else-if="col.data == 'file'">
                        <div class="mt-2" v-for="(item, index) in slotProps.data['list_file']">
                            <a target="_blank" :href="item.url" class="text-blue" :download="download(item.name)">{{
            item.name
        }}</a>
                        </div>
                    </template>
                    <template v-else-if="col.data == 'created_at'">
                        {{ formatDate(slotProps.data[col.data]) }}
                    </template>
                    <template v-else-if="col.data == 'created_by'">
                        <div v-if="slotProps.data.list_file[0]?.user_created_by" class="d-flex">
                            <Avatar :image="slotProps.data.list_file[0]?.user_created_by?.image_url"
                                :title="slotProps.data.list_file[0]?.user_created_by?.FullName" size="small"
                                shape="circle" /> <span class="align-self-center ml-2">{{
            slotProps.data.list_file[0]?.user_created_by?.FullName }}</span>
                        </div>
                    </template>

                    <template
                        v-else-if="col.data == 'action' && slotProps.data['is_user_upload'] == true && slotProps.data.list_file[0]?.user_created_by?.id == user.id && !model.is_chapnhan">
                        <a class="p-link text-danger font-16" @click="confirmDeleteDinhkem(slotProps.data)"><i
                                class="pi pi-trash"></i></a>
                    </template>
                    <template v-else>
                        {{ slotProps.data[col.data] }}
                    </template>
                </template>
            </Column>
        </DataTable>
        <Dialog v-model:visible="visibleDialog" header="Tạo mới" :modal="true" class="p-fluid">
            <div class="row mb-2">
                <div class="field col">
                    <label for="name">Mô tả:<span class="text-danger">*</span></label>
                    <InputText id="name" class="p-inputtext-sm" v-model.trim="modelfile.note" required="true"
                        :class="{ 'p-invalid': submitted && !modelfile.note }" />
                    <small class="p-error" v-if="submitted && !modelfile.note">Required.</small>
                </div>
            </div>
            <div class="row mb-2">
                <div class="field col">
                    <label for="name">Files:<span class="text-danger">*</span></label>
                    <div class="custom-file mt-2">
                        <input type="file" class="file-input" :id="'customFileup'" :multiple="true"
                            @change="fileChange($event)">
                        <label class="custom-file-label" :for="'customFileup'">Choose file</label>
                    </div>
                </div>
            </div>
            <template #footer>
                <Button label="Cancel" icon="pi pi-times" class="p-button-text" @click="hideDialog"></Button>
                <Button label="Save" icon="pi pi-check" class="p-button-text" @click="save"></Button>
            </template>
        </Dialog>
    </div>
</template>

<script setup>

import { onMounted, ref, watch, computed } from 'vue';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Button from 'primevue/button';
import Avatar from 'primevue/avatar';
import Dialog from 'primevue/dialog';
import InputText from 'primevue/inputtext';
import { storeToRefs } from 'pinia'

import { useDanhgianhacungcap } from '../../stores/danhgianhacungcap';
import danhgianhacungcapApi from '../../api/danhgianhacungcapApi';
import { useRoute } from 'vue-router';
import { formatDate, download } from '../../utilities/util';
import { useConfirm } from 'primevue/useconfirm';
import { useToast } from 'primevue/usetoast';
import { useAuth } from '../../stores/auth';


const toast = useToast();
const confirm = useConfirm();
const store_danhgianhacungcap = useDanhgianhacungcap();
const store_auth = useAuth();
const { user } = storeToRefs(store_auth);
const submitted = ref(false);
const modelfile = ref({});
const visibleDialog = ref();
const { files, model, waiting } = storeToRefs(store_danhgianhacungcap);
const route = useRoute();
const loading = ref(false);
const selected = ref();
const columns = ref([
    {
        label: "Mô tả",
        "data": "note",
        "className": "text-center",
    },
    {
        label: "Tập tin",
        "data": "file",
        "className": "text-center",
    },
    {
        label: "Ngày tạo",
        "data": "created_at",
        "className": "text-center"
    }, {
        label: "Người tạo",
        "data": "created_by",
        "className": "text-center"
    },
    {
        label: "Hành động",
        "data": "action",
        "className": "text-center"
    }
]);

const confirmDeleteDinhkem = (item) => {
    // console.log(item.dinhkem[key1]);
    confirm.require({
        message: 'Bạn có chắc muốn xóa?',
        header: 'Xác nhận',
        icon: 'pi pi-exclamation-triangle',
        accept: () => {

            waiting.value = true;
            var list_id = item.list_file.map((x) => {
                return x.id;
            });
            danhgianhacungcapApi.xoadinhkem({ list_id: list_id }).then((response) => {
                waiting.value = false;
                if (response.success) {
                    load();
                }

                // console.log(response)
            });


        }
    });

}
const save = () => {
    submitted.value = true;
    if (!modelfile.value.note) {
        alert("Chưa nhập mô tả!");
        return false;
    }
    var params = modelfile.value;
    params.danhgianhacungcap_id = model.value.id;
    var files = $(".file-input")[0].files;
    for (var stt = 0; stt < files.length; stt++) {
        var file = files[stt];
        params["file_" + stt] = file;
    }
    waiting.value = true;
    hideDialog();
    danhgianhacungcapApi.saveDinhkem(params).then((response) => {
        waiting.value = false;
        if (response.success) {
            toast.add({ severity: 'success', summary: 'Thành công!', detail: 'Thay đổi thành công', life: 3000 });
            load();
        }
        // console.log(response)
    });
}
const hideDialog = () => {
    visibleDialog.value = false;
    submitted.value = false;
};
const openNew = () => {
    visibleDialog.value = true;
    modelfile.value = {};
    submitted.value = false;
}
const fileChange = (e) => {
    var parents = $(e.target).parents(".custom-file");
    var label = $(".custom-file-label", parents);
    label.text(e.target.files.length + " Files");
}
const selectedColumns = computed(() => {
    return columns.value.filter(col => col.hide != true);
});
const load = async () => {
    var res = await danhgianhacungcapApi.getFiles(route.params.id);
    files.value = res;
}
onMounted(() => {
    load();
})
</script>
