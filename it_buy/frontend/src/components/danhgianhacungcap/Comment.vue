<template>
  <div class="card">
    <div class="card-header">
      <b>Thảo luận</b>
    </div>
    <div class="card-body">
      <form id="binhluan" enctype="multipart/form-data">
        <input name="danhgianhacungcap_id" :value="model.id" type="hidden" />

        <InputGroup>
          <input type="file" class="d-none" name='file[]' multiple @change="changeFile" />
          <DxHtmlEditor v-model:value="text" ref="editor_filecontent" :mentions="mentions_employee"
            style="width: 100%;">
          </DxHtmlEditor>
          <Button label="" icon="pi pi-file" size="small" severity="success" @click="AddCommentFile"></Button>

          <Button label="Gửi" icon="pi pi-send" size="small" @click="add_comment"></Button>
        </InputGroup>
        <div v-html="uploadText" v-if="uploadText" class="mt-2"></div>
      </form>
      <hr />
      <ul class="list-unstyled" id="comment_box">
        <li class="media comment_box my-2" :data-read="comment.is_read" v-for="(comment, index) in comments">
          <img class="mr-3 rounded-circle" :src="comment.user.image_url" width="50" alt="" />
          <div class="media-body border-bottom" style="display: grid">
            <h5 class="mt-0 mb-1" style="font-size: 14px;">
              {{ comment.user.fullName }}
              <small class="text-muted">
                -
                {{ formatDate(comment.created_at, "HH:mm DD/MM/YYYY") }}</small>
            </h5>
            <div class="mb-2" style="white-space: pre-wrap" v-html="comment.comment"></div>
            <div class="mb-2 attach_file file-box-content">
              <div class="file-box" v-for="(file, index1) in comment.files">
                <a :href="file.url" :download="download(file.name)" class="download-icon-link">
                  <i class="dripicons-download file-download-icon"></i>

                  <div class="text-center">
                    <i class="far fa-file text-danger"></i>
                    <h6 class="text-truncate" :title="file.name">
                      {{ file.name }}
                    </h6>
                    <small class="text-muted">{{ file.ext }}</small>
                  </div>
                </a>
              </div>
            </div>
          </div>
        </li>
      </ul>
      <div class="text-center load_more" @click="getComments()">
        <button class="btn btn-primary btn-sm px-5">Xem thêm bình luận</button>
      </div>
    </div>
  </div>
</template>
<script setup>
import { computed, onMounted, ref } from "vue";
import Api from "../../api/Api";
import { download, formatDate } from "../../utilities/util";
import { useDanhgianhacungcap } from "../../stores/danhgianhacungcap";
import { storeToRefs } from "pinia";
import danhgianhacungcapApi from "../../api/danhgianhacungcapApi";
import InputGroup from 'primevue/inputgroup';
import Button from 'primevue/button';

import {
  DxHtmlEditor,
} from 'devextreme-vue/html-editor';
import { useAuth } from "../../stores/auth";
import { useRoute } from "vue-router";
const route = useRoute();
const store = useAuth();
const { users } = storeToRefs(store);
const store_danhgianhacungcap = useDanhgianhacungcap();
const { model } = storeToRefs(store_danhgianhacungcap);
const comments = ref([]);
const text = ref();
const from_id = ref();
const uploadText = ref("");
const mentions_employee = computed(() => {
  return [
    {
      dataSource: users.value,
      searchExpr: "name",
      displayExpr: "name",
      valueExpr: "id",
      marker: "@",
    },
  ]
})
const getComments = async () => {
  /// Lấy comments
  var from_id;
  if (comments.value.length > 0) {
    from_id = comments.value[comments.value.length - 1].id;
  }
  var ress = await danhgianhacungcapApi.morecomment(route.params.id, from_id);
  var comments_tmp = ress.comments;
  if (comments_tmp.length == 10) {
    comments_tmp.pop();
  } else {
    $(".load_more").remove();
  }
  comments.value = comments.value.concat(comments_tmp);
}
const AddCommentFile = () => {
  $("[name='file[]']").click();
}
const changeFile = () => {
  uploadText.value = $("[name='file[]']")[0].files.length + " Files. Nhấn <i class='pi pi-send'></i> để gửi tin nhắn."
}
const add_comment = async (e) => {
  e.preventDefault();
  var comment = text.value;
  var files = $("[name='file[]']")[0].files;
  // console.log(comment);
  // return false;
  if (!comment && !files.length) {
    alert("Mời nhập bình luận!");
    return false;
  }
  var form = $("#binhluan")[0];
  var formData = new FormData(form);
  formData.append("comment", comment);
  var users_related = [];
  $(".dx-mention", comment).each(function () {
    // console.log(this)
    var user_id = $(this).data("id");
    users_related.push(users_related);
    formData.append("users_related[]", user_id);
  });
  // if (users_related.length > 0) {
  //   formData.append("users_related", users_related);
  // }
  // console.log(formData)
  $("#binhluan").trigger("reset");
  text.value = "";
  uploadText.value = null;
  var result = await danhgianhacungcapApi.addcomment(formData);
  if (result.success) {
    var comment = result.comment;
    comments.value.unshift(comment);
  }
}
onMounted(() => {
  getComments();
  store.fetchUsers();
})
</script>

<style lang="scss"></style>
